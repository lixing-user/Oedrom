using UnityEngine;

public class PanelView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ButtonView backButton;

    public event System.Action OnBackRequested;

    private void Awake()
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
