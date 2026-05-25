using TMPro;
using UnityEngine;

public interface ICameraHintView
{
    void ShowCameraHint(string message);
    void HideCameraHint();
}

public class CameraHintView : MonoBehaviour, ICameraHintView
{
    [SerializeField] private TextMeshProUGUI _cameraHintText;
    [SerializeField] private CanvasGroup _controlGroup;

    private void Show()
    {
        _controlGroup.alpha = 1f;
        _controlGroup.interactable = true;
        _controlGroup.blocksRaycasts = true;
    }
    private void Hide()
    {
        _controlGroup.alpha = 0f;
        _controlGroup.interactable = false;
        _controlGroup.blocksRaycasts = false;
    }

    public void ShowCameraHint(string message)
    {
        Show();
        if (string.IsNullOrEmpty(message))
        {
            HideCameraHint();
            return;
        }

        _cameraHintText.text = message;
    }

    public void HideCameraHint()
    {
        Hide();
    }
}