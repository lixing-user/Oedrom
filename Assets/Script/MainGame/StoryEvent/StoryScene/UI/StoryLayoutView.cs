using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryLayoutView : MonoBehaviour, IStoryView
{
    #region Fields and Properties
    [Header("Layout")]
    [SerializeField] private Image spriteLeft; // Sprite for the left character
    [SerializeField] private Image spriteRight; // Sprite for the right character
    [SerializeField] private Image spriteMiddle;
    [SerializeField] private Image background;

    [SerializeField] private TMP_Text nameText; 
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialogueBox;

    [Header("Character Size")]
    [SerializeField] private float normalScale = 1.0f;
    [SerializeField] private float largeScale = 1.25f;
    [SerializeField] private bool flipLeftSprite = true;

    [Header("Button")]
    [SerializeField] public ToggleViewS autoPlayToggle; 
    [SerializeField] public ButtonViewS skipRequestButton; 
    [SerializeField] public ButtonViewS skipConfirmButton; 
    [SerializeField] public ButtonViewS visibleButton; 
    [SerializeField] public ButtonViewS logButton; 
    [SerializeField] public ButtonViewS hideLogButton;
    [SerializeField] public ButtonViewS quitRequestButton;
    [SerializeField] public ButtonViewS quitConfirmButton;


    [Header("LogPanel")]
    [SerializeField] public PanelViewS logPanel; // Panel to display History dialogue log
    [SerializeField] public TMP_Text logPanelText;
    [SerializeField] private ScrollRect logScrollRect;

    [Header("SettingPanel")]
    [SerializeField] public PanelViewS quitPanel;

    [Header("SkipWarning")]
    [SerializeField] public PanelViewS skipWarningPanel;

    [Header("CG Display Components")]
    [SerializeField] private GameObject cgPanel;        // CG 面板
    [SerializeField] private Image cgImage;             // 顯示 CG 的 Image 組件
    [SerializeField] private CanvasGroup cgCanvasGroup; // 控制透明度

    [Header("Effects")]
    [SerializeField] private GameObject focusOverlay;

    // View Events
    public event System.Action<bool> OnAutoPlayToggled;
    public event System.Action OnVisibilityToggled;
    public event System.Action OnLogShowRequested;
    public event System.Action OnLogHideRequested;
    public event System.Action OnSkipRequested;
    public event System.Action OnSkipConfirmed;
    public event System.Action OnSkipCanceled;
    public event System.Action OnQuitRequested;
    public event System.Action OnQuitConfirmed;
    public event System.Action OnBackClicked;
    #endregion

    #region Initialization
    void Start()
    {
        autoPlayToggle.OnValueChanged += HandleAutoPlayToggled;
        visibleButton.OnClicked += () => OnVisibilityToggled?.Invoke();
        logButton.OnClicked += () => OnLogShowRequested?.Invoke();
        hideLogButton.OnClicked += () => OnLogHideRequested?.Invoke();
        skipRequestButton.OnClicked += () => OnSkipRequested?.Invoke();
        skipConfirmButton.OnClicked += () => OnSkipConfirmed?.Invoke();
        quitRequestButton.OnClicked += () => OnQuitRequested?.Invoke();
        quitConfirmButton.OnClicked += () => OnQuitConfirmed?.Invoke();

        quitPanel.OnBackRequested += () => OnBackClicked?.Invoke();
        skipWarningPanel.OnBackRequested += () => OnSkipCanceled?.Invoke();

        HideLogPanel();
        HideSkipWarningPanel();
        spriteLeft.gameObject.SetActive(false);
        spriteRight.gameObject.SetActive(false);
        spriteMiddle.gameObject.SetActive(false);
    }
    void OnDestroy()
    {
        autoPlayToggle.OnValueChanged -= HandleAutoPlayToggled;
        visibleButton.OnClicked -= () => OnVisibilityToggled?.Invoke();
        logButton.OnClicked -= () => OnLogShowRequested?.Invoke();
        hideLogButton.OnClicked -= () => OnLogHideRequested?.Invoke();
        skipRequestButton.OnClicked -= () => OnSkipRequested?.Invoke();
        skipConfirmButton.OnClicked -= () => OnSkipConfirmed?.Invoke();
        quitRequestButton.OnClicked -= () => OnQuitRequested?.Invoke();
        quitConfirmButton.OnClicked -= () => OnQuitConfirmed?.Invoke();

        quitPanel.OnBackRequested -= () => OnBackClicked?.Invoke();
        skipWarningPanel.OnBackRequested -= () => OnSkipCanceled?.Invoke();
    }
    #endregion

    #region 介面實作

    #region 視覺界面
    public void SetDialogue(string _name, string _dialogue)
    {
        nameText.text = _name;
        dialogueText.text = _dialogue;
    }

    public void SetCharacter(Sprite sprite, CharacterSpritePosition position, CharacterSpriteSize size)
    {
        Image target = GetSpriteSlot(position);
        if (target == null) return;

        target.sprite = sprite;
        target.rectTransform.localScale = GetSpriteScale(position, size);
        target.gameObject.SetActive(true);

        Debug.Log($"[View] SetCharacter {position}: {sprite?.name} ({size})");
    }
    private Image GetSpriteSlot(CharacterSpritePosition position) => position switch
    {
        CharacterSpritePosition.Left => spriteLeft,
        CharacterSpritePosition.Right => spriteRight,
        CharacterSpritePosition.Center => spriteMiddle,
        _ => null
    };
    private Vector3 GetSpriteScale(CharacterSpritePosition position, CharacterSpriteSize size)
    {
        float s = size == CharacterSpriteSize.Large ? largeScale : normalScale;
        float x = (position == CharacterSpritePosition.Left && flipLeftSprite) ? -s : s;
        return new Vector3(x, s, 1f);
    }

    public void ClearCharacter(CharacterSpritePosition position)
    {
        switch (position)
        {
            case CharacterSpritePosition.Left: spriteLeft.gameObject.SetActive(false); break;
            case CharacterSpritePosition.Right: spriteRight.gameObject.SetActive(false); break;
            case CharacterSpritePosition.Center: spriteMiddle.gameObject.SetActive(false); break;
        }
    }

    public void SetBackground(Sprite sprite)
    {
        background.sprite = sprite;
    }

    public void ChangeBackground(Sprite sprite, float fadeTime)
    {
        StartCoroutine(FadeBackground(sprite, fadeTime));
        Debug.Log($"[View] 切換背景: {sprite.name}");
    }
    private IEnumerator FadeBackground(Sprite newBg, float duration)
    {
        background.sprite = newBg;
        yield return null;
    }

    public void ShowCG(Sprite cgSprite, float fadeTime)
    {
        cgImage.sprite = cgSprite;
        cgPanel.SetActive(true);

        StopAllCoroutines(); // 停止之前的淡入淡出
        StartCoroutine(FadeCG(0, 1, fadeTime));

        Debug.Log($"[View] 顯示 CG: {cgSprite.name}");
    }

    public void HideCG(float fadeTime)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCG(1, 0, fadeTime, () =>
        {
            cgPanel.SetActive(false);
        }));

        Debug.Log("[View] 隱藏 CG");
    }

    private IEnumerator FadeCG(float from, float to, float duration, Action onComplete = null)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            cgCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cgCanvasGroup.alpha = to;
        onComplete?.Invoke();
    }

    public void SetDialogueEffect(DialogueEffect effect, CharacterSpritePosition activePosition)
    {
        Color normalColor = Color.white;
        Color darkenedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // 暗灰色

        switch (effect)
        {
            case DialogueEffect.None:
                // 關閉遮罩，所有立繪恢復正常亮度
                if (focusOverlay != null) focusOverlay.SetActive(false);
                spriteLeft.color = normalColor;
                spriteRight.color = normalColor;
                spriteMiddle.color = normalColor;
                break;

            case DialogueEffect.Think:
                // 開啟遮罩，只有當前說話者亮，其他人變暗
                if (focusOverlay != null) focusOverlay.SetActive(true);
                spriteLeft.color = (activePosition == CharacterSpritePosition.Left) ? normalColor : darkenedColor;
                spriteRight.color = (activePosition == CharacterSpritePosition.Right) ? normalColor : darkenedColor;
                spriteMiddle.color = (activePosition == CharacterSpritePosition.Center) ? normalColor : darkenedColor;
                break;

            case DialogueEffect.Mask:
                // 開啟遮罩，且「所有人」不論是誰都直接變暗
                if (focusOverlay != null) focusOverlay.SetActive(true);
                spriteLeft.color = darkenedColor;
                spriteRight.color = darkenedColor;
                spriteMiddle.color = darkenedColor;
                break;
        }
    }
    #endregion

    #region 互動界面
    public void ShowLogPanel(System.Collections.Generic.List<DialogueHistoryLine> history)
    {
        logPanel.Show();
        logPanelText.text = "";
        foreach (var line in history)
        {
            logPanelText.text += $"{line.character}: {line.content}\n";
        }

        Canvas.ForceUpdateCanvases();
        if (logScrollRect != null)
        {
            logScrollRect.verticalNormalizedPosition = 0f;
        }
    }
    public void HideLogPanel()
    {
        logPanel.Hide();
    }


    public void ShowSkipWarningPanel()
    {
        skipWarningPanel.Show();
    }
    public void HideSkipWarningPanel()
    {
        skipWarningPanel.Hide();
    }


    public void ShowQuitPanel()
    {
        quitPanel.Show();
    }
    public void HideQuitPanel()
    {
        quitPanel.Hide();
    }


    public void ToggleUIVisibility(bool visible)
    {
        nameText.gameObject.SetActive(visible);
        dialogueText.gameObject.SetActive(visible);
        autoPlayToggle.gameObject.SetActive(visible);
        skipRequestButton.gameObject.SetActive(visible);
        logButton.gameObject.SetActive(visible);
        hideLogButton.gameObject.SetActive(visible);
        visibleButton.gameObject.SetActive(visible);
        quitRequestButton.gameObject.SetActive(visible);
    }


    public void SetAutoPlayState(bool isOn)
    {
        autoPlayToggle.SetIsOnWithoutNotify(isOn);
    }

    private void HandleAutoPlayToggled(bool isOn) => OnAutoPlayToggled?.Invoke(isOn);
    #endregion

    #endregion
}


public interface IStoryView
{
    event System.Action<bool> OnAutoPlayToggled;
    event System.Action OnVisibilityToggled;
    event System.Action OnLogShowRequested;
    event System.Action OnLogHideRequested;
    event System.Action OnSkipRequested;
    event System.Action OnSkipConfirmed;
    event System.Action OnSkipCanceled;
    event System.Action OnQuitRequested;
    event System.Action OnQuitConfirmed;
    event System.Action OnBackClicked;

    void SetDialogue(string _name, string _dialogue);
    void SetCharacter(Sprite sprite, CharacterSpritePosition position, CharacterSpriteSize size);
    void ClearCharacter(CharacterSpritePosition position);
    void SetBackground(Sprite sprite);
    void ChangeBackground(Sprite sprite, float fadeTime);
    void ShowCG(Sprite Sprite, float fadeTime);
    void HideCG(float fadeTime);
    void SetDialogueEffect(DialogueEffect effect, CharacterSpritePosition activePosition);
    void ShowLogPanel(System.Collections.Generic.List<DialogueHistoryLine> history);
    void HideLogPanel();
    void ShowSkipWarningPanel();
    void HideSkipWarningPanel();
    void ShowQuitPanel();
    void HideQuitPanel();
    void ToggleUIVisibility(bool visible);
    void SetAutoPlayState(bool isOn);
}