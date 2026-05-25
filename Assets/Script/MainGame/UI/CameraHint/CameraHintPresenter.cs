using UnityEngine;
using Zenject;

public class CameraHintPresenter : MonoBehaviour
{
    [Inject] protected readonly SignalBus _signalBus;
    protected ICameraHintView _view;
    protected CameraMode _currentCameraMode = CameraMode.Default;


    #region Lifecycle
    protected virtual void Awake()
    {
        _view = GetComponent<ICameraHintView>();
        _signalBus.Subscribe<CameraModeChangedSignal>(OnCameraModeChanged);
    }

    protected virtual void OnDestroy()
    {
        _signalBus.Unsubscribe<CameraModeChangedSignal>(OnCameraModeChanged);
    }
    #endregion

    private void OnCameraModeChanged(CameraModeChangedSignal signal)
    {
        _currentCameraMode = signal.NewMode;
        UpdateCameraHint(_currentCameraMode);
    }

    protected virtual void UpdateCameraHint(CameraMode mode)
    {
        string hintText = "";

        switch (mode)
        {
            case CameraMode.Default:
                hintText = "[預設視角]\nSpace/WASD: 自由 | T: 俯視 | 右鍵/F: 聚焦";
                break;

            case CameraMode.TopView:
                hintText = "[俯視角]\nEsc 或 T: 返回預設視角";
                break;

            case CameraMode.Freelook:
                hintText = "[自由視角]\nEsc: 返回預設視角 | Q/E: 旋轉 | 滾輪: 縮放";
                break;

            case CameraMode.Focus:
                hintText = "[聚焦視角]\nEsc: 返回預設視角";
                break;

            case CameraMode.FocusEnemy:
                hintText = "[聚焦敵人視角]\nEsc: 返回預設視角";
                break;

            case CameraMode.Opening:
            case CameraMode.Uninitialized:
            default:
                hintText = ""; // 開場運鏡或未初始化時不顯示提示
                break;
        }

        _view.ShowCameraHint(hintText);
    }
}
