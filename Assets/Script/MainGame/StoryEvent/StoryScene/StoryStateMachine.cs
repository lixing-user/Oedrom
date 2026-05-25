using System.Collections.Generic;
using UnityEngine;
using Zenject;

public enum StoryPhaseType
{
    Prologue,      // 序章對話
    MainDialogue,  // 主要劇情
    Epilogue,      // 結尾
    TransitionToGame // 轉換到遊戲場景
}

public interface IStoryPhaseState
{
    void OnEnter(StoryStateMachine machine);
    void OnUpdate(StoryStateMachine machine);
    void OnExit(StoryStateMachine machine);
}


public class StoryStateMachine : MonoBehaviour
{
    [Inject] private DiContainer _container;
    [Inject] private SignalBus _signalBus;
    [Inject] private StoryProgressModel _progressModel;
    [Inject] private StoryProgressManager _progressManager;
    [Inject] private IStoryPresenter _presenter;

    private Dictionary<StoryPhaseType, IStoryPhaseState> _phases;
    private IStoryPhaseState _currentPhase;

    private StoryChapterConfig _currentChapter;
    private int _currentPhaseIndex = 0;

    #region Binding & Initialization
    private void Awake()
    {
        InitializePhases();
        SubscribeToSignals();
    }
    private void OnDestroy()
    {
        UnsubscribeFromSignals();
    }

    private void InitializePhases()
    {
        _phases = new Dictionary<StoryPhaseType, IStoryPhaseState>
        {
            { StoryPhaseType.Prologue, _container.Instantiate<ProloguePhase>() },
            { StoryPhaseType.MainDialogue, _container.Instantiate<MainDialoguePhase>() },
            { StoryPhaseType.Epilogue, _container.Instantiate<EpiloguePhase>() },
        //    { StoryPhaseType.TransitionToGame, new TransitionPhase() }
        };
    }

    private void SubscribeToSignals()
    {
        //_signalBus.Subscribe<StartChapterSignal>(OnStartChapter);
        _signalBus.Subscribe<DialogueEndedSignal>(OnDialogueEnded);
        //_signalBus.Subscribe<ProgressPhaseSignal>(OnProgressPhase);
    }
    private void UnsubscribeFromSignals()
    {
        //_signalBus.Unsubscribe<StartChapterSignal>(OnStartChapter);
        _signalBus?.Unsubscribe<DialogueEndedSignal>(OnDialogueEnded);
        //_signalBus.Unsubscribe<ProgressPhaseSignal>(OnProgressPhase);
    }
    #endregion

    #region 核心業務邏輯
    // ===== 公開方法：由外部啟動章節 =====
    public void StartChapter(StoryChapterConfig chapter)
    {
        _currentChapter = chapter;
        _currentPhaseIndex = 0;

        _progressManager.SetCurrentChapter(chapter);    // 將當前章節同步給 Manager，這樣 StateMachine 之後查的時候才拿得到資料

        Debug.Log($"[SSM] > StartChapter: {chapter.chapterName} | PhaseCount: {chapter.phaseSequence.Length}");

        // 開始第一個階段
        if (chapter.phaseSequence.Length > 0)
        {
            ChangePhase(chapter.phaseSequence[0]);
        }
    }

    // ===== 階段切換邏輯 =====
    private void ChangePhase(StoryPhaseType newPhase)
    {
        _currentPhase?.OnExit(this);
        _currentPhase = _phases[newPhase];
        _currentPhase.OnEnter(this);

        Debug.Log($"[StateMachine] 進入階段: {newPhase}");
    }


    // ===== Signal 處理：對話結束時觸發 =====
    private void OnDialogueEnded(DialogueEndedSignal signal)
    {
        Debug.Log($"[StateMachine] 收到對話結束信號: {signal.ChapterId}");

        ProgressToNextPhase();
    }

    public void ProgressToNextPhase()
    {
        _currentPhaseIndex++;

        if (_currentChapter == null)
        {
            Debug.LogError($"[SSM] ProgressToNextPhase 被呼叫，但 _currentChapter 是 null！PhaseIndex: {_currentPhaseIndex}");
            return;
        }

        if (_currentPhaseIndex < _currentChapter.phaseSequence.Length)
        {
            ChangePhase(_currentChapter.phaseSequence[_currentPhaseIndex]);
        }
        else
        {
            OnChapterComplete();
        }
    }

    private void OnChapterComplete()
    {
        Debug.Log($"[StateMachine] 章節完成: {_currentChapter.chapterName}");

        // 記錄進度
        _progressModel.MarkChapterComplete(_currentChapter.chapterId);

        //// 檢查是否有下一章節
        //if (_currentChapter.nextChapter != null)
        //{
        //    StartChapter(_currentChapter.nextChapter);
        //}
        //else
        //{
        //    // 發出信號：所有章節完成，可切換到遊戲場景
        //    _signalBus.Fire(new AllChaptersCompletedSignal());
        //}
        string nextId = _currentChapter.nextChapter != null ? _currentChapter.nextChapter.chapterId : null;

        _signalBus.Fire(new StoryChapterCompletedSignal
        {
            CompletedChapterId = _currentChapter.chapterId,
            NextChapterId = nextId
        });
    }
    #endregion

    #region Getters Methods
    public StoryProgressModel GetProgressModel() => _progressModel;
    public StoryProgressManager GetProgressManager() => _progressManager;
    //public StoryPhaseFacade GetFacade() => _facade;
    public StoryChapterConfig GetCurrentChapter() => _currentChapter;
    public StoryPhaseType GetCurrentPhaseType() => _currentChapter.phaseSequence[_currentPhaseIndex];
    public IStoryPresenter GetPresenter() => _presenter;
    #endregion
}
