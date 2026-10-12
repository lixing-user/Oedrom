using System;
using System.Collections;
using UnityEngine;
using Zenject;

public interface IStoryPresenter
{
    void LoadChapter(StoryChapterConfig config);
}


public class StoryLayoutPresenter : MonoBehaviour, IStoryPresenter
{
    [Inject] private SignalBus _signalBus;
    [Inject] private IDialogueService _dialogueService;
    [Inject] private ISpriteService _spriteService; 

    private IStoryView _view;
    private StoryInputHandler _inputHandler;
    private StoryRuntimeModel _model;
    private StoryChapterConfig _currentConfig;

    private float _autoPlayInterval = 3f;
    private Coroutine _autoPlayCoroutine;

    private Coroutine _skipCoroutine;
    private float _skipInterval = 0.05f;

    #region 初始化&綁定
    public void LoadChapter(StoryChapterConfig config)
    {
        _currentConfig = config;

        _dialogueService.LoadDialogue(config.dialogueFile);
        _spriteService.LoadFromConfig(config);

        // 初始化 Model
        _model = new StoryRuntimeModel();
        _model.currentDialogueIndex = 0;
        SetAutoPlay(false);

        // 從 Config 讀取資源並設置到 View
        _view.SetBackground(_spriteService.GetBackgroundSprite(config.initialBackground));

        // 顯示第一句對話
        AdvanceDialogue();

        Debug.Log($"[StoryPresenter] 載入章節: {config.chapterName}");
    }


    void Start()
    {
        _view = GetComponent<IStoryView>();
        _inputHandler = GetComponent<StoryInputHandler>();

        BindViewEvents();
        BindInputEvents();
    }

    void OnDestroy()
    {
        UnbindViewEvents();
        UnbindInputEvents();
    }

    private void BindViewEvents()
    {
        _view.OnAutoPlayToggled += OnAutoPlayToggled;
        _view.OnVisibilityToggled += OnVisibilityToggled;
        _view.OnLogShowRequested += OnLogShowRequested;
        _view.OnLogHideRequested += OnLogHideRequested;
        _view.OnSkipRequested += OnSkipDialogueRequested;
        _view.OnSkipConfirmed += OnSkipDialogue;
        _view.OnQuitRequested += OnQuitRequested;
        _view.OnQuitConfirmed += OnQuitConfirmed;
        _view.OnBackClicked += OnBackClicked;

        _view.OnSkipCanceled += OnSkipDialogueCanceled;
    }

    private void UnbindViewEvents()
    {
        _view.OnAutoPlayToggled -= OnAutoPlayToggled;
        _view.OnVisibilityToggled -= OnVisibilityToggled;
        _view.OnLogShowRequested -= OnLogShowRequested;
        _view.OnLogHideRequested -= OnLogHideRequested;
        //此處的skip為藉由點按按鈕來直接跳過劇情
        _view.OnSkipRequested -= OnSkipDialogueRequested;
        _view.OnSkipConfirmed -= OnSkipDialogue;
        _view.OnQuitRequested -= OnQuitRequested;
        _view.OnQuitConfirmed -= OnQuitConfirmed;
        _view.OnBackClicked -= OnBackClicked;

        _view.OnSkipCanceled -= OnSkipDialogueCanceled;
    }

    private void BindInputEvents()
    {
        _inputHandler.OnAdvanceRequested += OnAdvanceRequested;
        _inputHandler.OnSkipStarted += OnFastForwardStarted;
        _inputHandler.OnSkipEnded += OnFastForwardEnded;
    }

    private void UnbindInputEvents()
    {
        // 此處的skip為藉由點按ctrl鍵來快速瀏覽
        _inputHandler.OnAdvanceRequested -= OnAdvanceRequested;
        _inputHandler.OnSkipStarted -= OnFastForwardStarted;
        _inputHandler.OnSkipEnded -= OnFastForwardEnded;
    }
    #endregion

    #region View Event 處理
    private void OnAdvanceRequested()
    {
        if (!_model.isUIVisible)
        {
            _model.isUIVisible = true;
            _view.ToggleUIVisibility(true);
            return;
        }

        if (_model.isAutoPlay) return;

        AdvanceDialogue();
    }

    private void OnAutoPlayToggled(bool isOn) => SetAutoPlay(isOn);
    private void SetAutoPlay(bool on)
    {
        _model.isAutoPlay = on;
        _view.SetAutoPlayState(on);   // WithoutNotify，不會回圈
        if (on) StartAutoPlay(); else StopAutoPlay();
    }

    private void OnVisibilityToggled()
    {
        _model.isUIVisible = !_model.isUIVisible; // 切換 UI 可見性邏輯
        _view.ToggleUIVisibility(_model.isUIVisible);
    }

    private void OnLogShowRequested()
    {
        _view.ShowLogPanel(_model.dialogueHistory);
    }

    private void OnLogHideRequested()
    {
        _view.HideLogPanel();
    }

    private void OnSkipDialogueRequested()
    {
        _view.ShowSkipWarningPanel();
    }

    private void OnSkipDialogue()
    {
        // 1.停止所有正在進行的播放與等待
        StopAutoPlay();
        OnFastForwardEnded(); // 確保快速閱讀也被關閉
        StopAllCoroutines();

        // 2. 隱藏確認跳過的警告面板
        _view.HideSkipWarningPanel();

        // 3. 直接呼叫結束邏輯，發送 Signal 給外部的狀態機
        OnChapterComplete();
    }

    private void OnSkipDialogueCanceled()
    {
        _view.HideSkipWarningPanel();
    }

    private void OnQuitRequested()
    {
        _view.ShowQuitPanel();
    }

    private void OnQuitConfirmed()
    {
        // 1. 停止所有正在進行的播放與等待
        StopAutoPlay();
        OnFastForwardEnded(); // 確保快速閱讀也被關閉
        StopAllCoroutines();

        // 2. 發送退出遊戲的 Signal 給外部的狀態機
        _signalBus.Fire(new RequestSessionStateChangeSignal
        {
            TargetState = GameSessionState.MainMenu
        });
    }

    private void OnBackClicked()
    {
        _view.HideLogPanel();
        _view.HideQuitPanel();
        _view.HideSkipWarningPanel();
    }
    #endregion

    #region 業務實作:對話流程控制
    private void AdvanceDialogue()
    {
        // 從 Service 獲取當前對話
        DialogueLine currentLine = _dialogueService.GetDialogueById(_model.currentDialogueIndex);

        if (currentLine == null)
        {
            Debug.LogWarning($"[Presenter] 找不到對話 ID: {_model.currentDialogueIndex}");
            return;
        }

        switch (currentLine.type)
        {
            case "#":
                HandleNormalDialogue(currentLine);
                break;

            case "CG":
                HandleCG(currentLine);
                break;

            case "BG":
                HandleBackgroundChange(currentLine);
                break;

            case "END":
                OnChapterComplete();
                break;

            case "ENTER": HandleEnter(currentLine); break;
            case "LEAVE": HandleLeave(currentLine); break;

            default:
                Debug.LogWarning($"[Presenter] 未處理的類型: {currentLine.type}");
                _model.currentDialogueIndex = currentLine.nextId;
                AdvanceDialogue(); // 跳過未知類型
                break;
        }
    }

    private void HandleNormalDialogue(DialogueLine line)
    {
        DialogueEffect currentEffect = DialogueEffect.None;

        if (!string.IsNullOrEmpty(line.effectTag))
        {
            if (line.effectTag.Equals("Think", System.StringComparison.OrdinalIgnoreCase))
            {
                currentEffect = DialogueEffect.Think;
            }
            else if (line.effectTag.Equals("Mask", System.StringComparison.OrdinalIgnoreCase))
            {
                currentEffect = DialogueEffect.Mask;
            }
        }
        _view.SetDialogueEffect(currentEffect, line.charPos);

        // 更新 Model
        _model.currentSpeakerName = line.characterName;
        _model.currentDialogueText = line.content;

        // 記錄歷史
        _model.AddDialogueToHistory(line.characterName, line.content);

        // 更新 View
        _view.SetDialogue(line.characterName, line.content);

        if (line.charPos != CharacterSpritePosition.None)
            ShowCharacterOnStage(line.characterName, line.charPos, line.charSize);
        _model.currentDialogueIndex = line.nextId;
    }

    private void HandleCG(DialogueLine line)
    {
        // 如果內容為空，或是寫了 None / Off，就執行「關閉 CG」
        if (string.IsNullOrEmpty(line.content) ||
        line.content.Equals("None", StringComparison.OrdinalIgnoreCase) ||
        line.content.Equals("Off", StringComparison.OrdinalIgnoreCase))
        {
            _view.HideCG(1.0f);
        }
        else
        {
            // 從 Config 動態獲取 CG
            Sprite cgSprite = _spriteService.GetCGSprite(line.content);
            if (cgSprite != null)
            {
                _view.ShowCG(cgSprite, 1.0f);
            }
            else
            {
                Debug.LogWarning($"[Presenter] 找不到 CG: {line.content}");
            }
        }

        _model.currentDialogueIndex = line.nextId; // 自動進入下一句
        StartCoroutine(DelayedAdvance(1.5f)); // 等待 CG 顯示完成
    }

    private void HandleBackgroundChange(DialogueLine line)
    {
        // 清空畫面上所有的立繪（左、中、右）
        _view.ClearCharacter(CharacterSpritePosition.Left);
        _view.ClearCharacter(CharacterSpritePosition.Center);
        _view.ClearCharacter(CharacterSpritePosition.Right);

        // 清空 Model 裡的位置記錄
        _model.ClearAllCharacters();

        // 強制把可能還在畫面上的 CG 關掉
        _view.HideCG(0.5f);

        Sprite bgSprite = _spriteService.GetBackgroundSprite(line.content);
        if (bgSprite != null)
        {
            _view.ChangeBackground(bgSprite, 1.0f);
        }
        else
        {
            Debug.LogWarning($"[Presenter] 找不到背景: {line.content}");
        }

        // 自動進入下一句
        _model.currentDialogueIndex = line.nextId;
        StartCoroutine(DelayedAdvance(1.0f)); // 等待背景切換
    }

    private void HandleEnter(DialogueLine line)
    {
        if (line.charPos == CharacterSpritePosition.None)
            Debug.LogWarning($"[Presenter] ENTER 缺少位置，ID={line.id}");
        else
            ShowCharacterOnStage(line.characterName, line.charPos, line.charSize);

        _model.currentDialogueIndex = line.nextId;
        AdvanceDialogue(); // 不顯示文字、不進 Log，直接接下一列
    }
    private void ShowCharacterOnStage(string name, CharacterSpritePosition pos, CharacterSpriteSize? csvSize)
    {
        Sprite sprite = _spriteService.GetCharacterSprite(name);
        if (sprite == null)
        {
            Debug.LogWarning($"[Presenter] 找不到角色 Sprite: '{name}'");
            return;
        }

        var oldPos = _model.FindCharacter(name);
        if (oldPos.HasValue && oldPos.Value != pos)
            _view.ClearCharacter(oldPos.Value);

        CharacterSpriteSize size = csvSize ?? _model.GetCharacterSize(name); // CSV 有填用 CSV，否則沿用

        _model.PlaceCharacter(name, pos, size);
        _view.SetCharacter(sprite, pos, size);
    }
    private void HandleLeave(DialogueLine line)
    {
        var pos = _model.FindCharacter(line.characterName);
        if (pos.HasValue)
        {
            _view.ClearCharacter(pos.Value);
            _model.RemoveCharacter(line.characterName);
        }
        else
            Debug.LogWarning($"[Presenter] LEAVE：'{line.characterName}' 不在場上，ID={line.id}");

        _model.currentDialogueIndex = line.nextId;
        AdvanceDialogue();
    }


    private void OnChapterComplete()
    {
        Debug.Log($"[Presenter] 章節 {_currentConfig.chapterName} 完成");
        _signalBus.Fire(new DialogueEndedSignal
        {
            ChapterId = _currentConfig.chapterId,
            IsLastChapter = _currentConfig.nextChapter == null
        });
    }

    private IEnumerator DelayedAdvance(float delay)
    {
        yield return new WaitForSeconds(delay);
        AdvanceDialogue();
    }
    #endregion

    #region 業務實作:自動播放
    public void StartAutoPlay()
    {
        if (_autoPlayCoroutine != null)
            StopCoroutine(_autoPlayCoroutine);

        _autoPlayCoroutine = StartCoroutine(AutoPlayRoutine());
    }
    private void StopAutoPlay()
    {
        if (_autoPlayCoroutine != null)
        {
            StopCoroutine(_autoPlayCoroutine);
            _autoPlayCoroutine = null;
            SetAutoPlay(false);
        }
    }
    IEnumerator AutoPlayRoutine()
    {
        while (_model.isAutoPlay)
        {
            AdvanceDialogue();
            yield return new WaitForSeconds(_autoPlayInterval);
        }

        _autoPlayCoroutine = null;
    }
    #endregion

    #region 業務實作:快速閱讀
    private void OnFastForwardStarted()
    {
        if (_model.isSkipping) return; // 避免重複啟動
        _model.isSkipping = true;

        if (_model.isAutoPlay) SetAutoPlay(false);

        if (_skipCoroutine != null) StopCoroutine(_skipCoroutine);
        _skipCoroutine = StartCoroutine(FastForwardRoutine());
    }

    private void OnFastForwardEnded()
    {
        _model.isSkipping = false;
        if (_skipCoroutine != null)
        {
            StopCoroutine(_skipCoroutine);
            _skipCoroutine = null;
        }
    }

    private IEnumerator FastForwardRoutine()
    {
        while (_model.isSkipping)
        {
            AdvanceDialogue();
            yield return new WaitForSeconds(_skipInterval);
        }
        _skipCoroutine = null;
    }
    #endregion
}