using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class CameraPresenterS : MonoBehaviour, ICameraPresenter
{
    [Header("Virtual Cameras")]
    [SerializeField] CinemachineCamera vcamOp;
    [SerializeField] CinemachineCamera vcam;
    [SerializeField] CinemachineCamera vcamf;
    [SerializeField] CinemachineCamera vcamFL;

    [Header("Locators")]
    [SerializeField] GameObject vcamFLLocator;

    [Header("Opening Animation")]
    [SerializeField] Transform openingStartPoint;
    [SerializeField] Transform openingEndPoint;
    [SerializeField] float openingDuration = 3f;

    [Header("Dynamic Focus Camera")]
    [SerializeField] CinemachineCamera vcamPieceFocus; // 可以在 Inspector 中掛載一個專門用來特寫的攝影機
    [SerializeField] float defaultFocusFOV = 15f;

    [Inject] private SignalBus _signalBus;

    // 使用 HashSet 儲存所有要求上鎖的來源 ID
    private HashSet<string> _lockTokens = new HashSet<string>();
    public bool IsLocked => _lockTokens.Count > 0;

    private CameraMode _currentMode = CameraMode.Uninitialized;
    private FreelookCameraController _freelookController;
    public CameraMode CurrentMode => _currentMode;

    void Start()
    {
        _signalBus.Subscribe<CameraFocusRequestSignal>(OnFocusRequested);
        _signalBus.Subscribe<CameraLockStateSignal>(OnCameraLockStateChanged);
        _signalBus.Subscribe<CameraModeRequestSignal>(OnCameraModeRequested);
        _signalBus.Subscribe<ResetCameraToDefaultSignal>(signal => RequestDefaultMode(true));

        _freelookController = vcamFLLocator.GetComponent<FreelookCameraController>();

        ResetAllCameraPriorities();
        TransitionTo(CameraMode.Default);
    }

    void OnDestroy()
    {
        _signalBus.Unsubscribe<CameraFocusRequestSignal>(OnFocusRequested);
        _signalBus.Unsubscribe<CameraLockStateSignal>(OnCameraLockStateChanged);
        _signalBus.Unsubscribe<CameraModeRequestSignal>(OnCameraModeRequested);
        _signalBus.Unsubscribe<ResetCameraToDefaultSignal>(signal => RequestDefaultMode(true));
    }

    private void ResetAllCameraPriorities()
    {
        vcamOp.Priority = 0;
        vcam.Priority = 0;
        vcamf.Priority = 0;
        vcamFL.Priority = 0;

        Debug.Log("Reset all camera priorities to 0");
    }

    private void TransitionTo(CameraMode newMode)
    {
        if (_currentMode == newMode) return;

        _currentMode = newMode;
        OnModeChanged(newMode);
        Debug.Log($"Transitioned to {newMode} mode");
    }

    private void OnModeChanged(CameraMode mode)
    {
        switch (mode)
        {
            case CameraMode.Opening:
                ApplyOpeningMode();
                break;
            case CameraMode.Default:
                ApplyDefaultMode();
                break;
            case CameraMode.Freelook:
                ApplyFreelookMode();
                break;
            case CameraMode.Focus:
                ApplyPieceFocusMode();
                break;
            case CameraMode.StandbyZone:
                ApplyStandbyZoneMode();
                break;
        }
    }

    private void OnCameraModeRequested(CameraModeRequestSignal signal)
    {
        if (IsLocked) return;

        switch (signal.TargetMode)
        {
            case CameraMode.StandbyZone:
                RequestStandbyZoneMode();
                break;
            case CameraMode.TopView:
                RequestTopViewMode();
                break;
            case CameraMode.Default:
                RequestDefaultMode();
                break;
            default:
                Debug.LogWarning($"[CameraPresenterS] 透過 Signal 請求了未支援直接切換的模式: {signal.TargetMode}");
                break;
        }
    }

    private void OnFocusRequested(CameraFocusRequestSignal signal)
    {
        RequestPieceFocusMode(signal.TargetTransform);
    }

    private void OnCameraLockStateChanged(CameraLockStateSignal signal)
    {
        LockCamera(signal.RequesterId, signal.IsLocked);
    }

    #region 業務實作
    private void ApplyOpeningMode()
    {
        vcamOp.Priority = 50;
        vcam.Priority = 0;
        vcamf.Priority = 0;
        vcamFL.Priority = 0;

        if (_freelookController != null)
            _freelookController.enabled = false;
    }

    private void ApplyDefaultMode()
    {
        vcamOp.Priority = 0;
        vcam.Priority = 10;
        vcamf.Priority = 0;
        vcamFL.Priority = 0;
        vcamPieceFocus.Priority = 0; // 確保 PieceFocus 攝影機不會干擾 Default

        vcamf.LookAt = null;
        vcamf.Follow = null;

        vcamPieceFocus.LookAt = null;
        vcamPieceFocus.Follow = null;

        if (_freelookController != null)
            _freelookController.enabled = false;
        Debug.Log("Switched to Default Camera Mode");
    }

    private void ApplyPieceFocusMode()
    {
        vcamOp.Priority = 0;
        vcam.Priority = 0;
        vcamf.Priority = 0;
        vcamFL.Priority = 0;
        vcamPieceFocus.Priority = 30; // 確保優先級高於 Default
        vcamPieceFocus.Lens.FieldOfView = defaultFocusFOV;

        if (_freelookController != null)
            _freelookController.enabled = false;

        Debug.Log("Switched to Piece Focus Camera Mode");
    }

    private void ApplyStandbyZoneMode()
    {
        vcamOp.Priority = 0;
        vcam.Priority = 0;
        vcamf.Priority = 20;
        vcamFL.Priority = 0;

        if (_freelookController != null)
            _freelookController.enabled = false;
        Debug.Log("Switched to Focus StandbyZone Camera Mode");
    }

    private void ApplyFreelookMode()
    {
        vcamOp.Priority = 0;
        vcam.Priority = 0;
        vcamf.Priority = 0;
        vcamFL.Priority = 15;

        if (_freelookController != null)
            _freelookController.enabled = true;
        Debug.Log("Switched to Topview Freelook Camera Mode");
    }
    #endregion

    #region 外部委託
    public void RequestFreelookMode()
    {
        if (IsLocked) return;
        TransitionTo(CameraMode.Freelook);
    }

    public void RequestDefaultMode(bool force = false)
    {
        if (!force && IsLocked) return;
        TransitionTo(CameraMode.Default);
    }

    public void RequestTopViewMode()
    {
        if (IsLocked) return;
        // S 版本沒有 TopView 攝影機，降級處理為 Default
        // 或直接留空，視場景需求決定
        RequestDefaultMode();
    }

    public void RequestPieceFocusMode(Transform target)
    {
        if (IsLocked) return;
        if (target == null)
        {
            RequestDefaultMode();
            return;
        }

        // 1. 設定 Cinemachine 的 LookAt 目標
        vcamPieceFocus.LookAt = target;
        vcamPieceFocus.Follow = target;

        TransitionTo(CameraMode.Focus);
    }

    public void RequestEnemyFocusMode(Transform target)
    {
        if (IsLocked) return;
        RequestDefaultMode();
    }

    public void RequestStandbyZoneMode()
    {
        if (IsLocked) return;
        // S 版本的 Focus 是固定 StandbyZone，不需要 target
        // 忽略 target，呼叫內部固定邏輯
        TransitionTo(CameraMode.StandbyZone);
    }

    public void LockCamera(string requesterId, bool isLocked)
    {
        if (string.IsNullOrEmpty(requesterId))
        {
            Debug.LogWarning("[Camera] 嘗試鎖定鏡頭，但未提供 requesterId！");
            return;
        }

        if (isLocked)
        {
            _lockTokens.Add(requesterId);
            Debug.Log($"[Camera] 鏡頭已鎖定。來源: {requesterId}。目前鎖定數量: {_lockTokens.Count}");
        }
        else
        {
            _lockTokens.Remove(requesterId);
            Debug.Log($"[Camera] 鏡頭已解鎖。來源: {requesterId}。目前鎖定數量: {_lockTokens.Count}");
        }
    }
    #endregion

    #region Opening運鏡邏輯
    public void PlayOpeningSequence()
    {
        StartCoroutine(OpeningRoutine());
    }

    private System.Collections.IEnumerator OpeningRoutine()
    {
        Debug.Log("[Camera] 開始開場運鏡...");
        TransitionTo(CameraMode.Opening); 

        // 等待運鏡時間
        // (如果你有實作 openingStartPoint 到 EndPoint 的移動邏輯，也寫在這裡)
        yield return new WaitForSeconds(openingDuration);

        Debug.Log("[Camera] 運鏡結束，切換回預設視角...");
        TransitionTo(CameraMode.Default);

        // 通知 TutorialManager 開始教學！
        Debug.Log("[Camera] 發送 StartTutorialSignal");
    }
    #endregion
}


public struct CameraFocusRequestSignal
{
    public Transform TargetTransform;

    public CameraFocusRequestSignal(Transform target)
    {
        TargetTransform = target;
    }
}

public struct CameraModeRequestSignal
{
    public CameraMode TargetMode;

    public CameraModeRequestSignal(CameraMode mode)
    {
        TargetMode = mode;
    }
}

public struct ResetCameraToDefaultSignal { }