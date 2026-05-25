using UnityEngine;

/// <summary>
/// 故事模式專用Panel欄位
/// </summary>
public class PanelViewS : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ButtonViewS backButton;

    public event System.Action OnBackRequested;

    private void Start()
    {
        canvasGroup = this.GetComponent<CanvasGroup>();

        if (backButton != null)
        {
            backButton.OnClicked += HandleBackClick;
        }

        Hide();
    }

    private void HandleBackClick()
    {
        OnBackRequested?.Invoke();
    }

    public void Show()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
