using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhaseControlView : MonoBehaviour, IPhaseControlView
{
    [Header("Phase Control UI")]
    [SerializeField] private Button _endPhaseButton;
    [SerializeField] private GameObject _phaseHintPanel;
    [SerializeField] private TextMeshProUGUI _phaseHintText;
    [SerializeField] private CanvasGroup _controlGroup;

    public event System.Action OnEndPhaseRequested;


    void Start()
    {
        _endPhaseButton.onClick.AddListener(() => OnEndPhaseRequested?.Invoke());
        Hide();
    }

    void OnDestroy()
    {
        _endPhaseButton.onClick.RemoveAllListeners();
    }

    #region 業務實作
    public void Show()
    {
        _controlGroup.alpha = 1f;
        _controlGroup.interactable = true;
        _controlGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        _controlGroup.alpha = 0f;
        _controlGroup.interactable = false;
        _controlGroup.blocksRaycasts = false;
    }

    public void SetButtonInteractable(bool interactable)
    {
        _endPhaseButton.interactable = interactable;
    }

    public void ShowPhaseHint(string message)
    {
        _phaseHintText.text = message;
    }
    #endregion
}

public interface IPhaseControlView
{
    event System.Action OnEndPhaseRequested;
    void Show();
    void Hide();
    void SetButtonInteractable(bool interactable);
    void ShowPhaseHint(string message);
}