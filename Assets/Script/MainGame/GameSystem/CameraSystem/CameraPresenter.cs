using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;


public enum CameraMode 
{ 
    Uninitialized,
    Opening,
    Default,
    TopView,
    Freelook, 
    Focus,
    FocusEnemy,
    StandbyZone
}

public class CameraPresenter : MonoBehaviour, ICameraPresenter
{
    [Header("Virtual Cameras")]
    [SerializeField] CinemachineCamera vcamOp;
    [SerializeField] CinemachineCamera vcam;
    [SerializeField] CinemachineCamera vcamt;
    [SerializeField] CinemachineCamera vcamf;
    [SerializeField] CinemachineCamera vcamFL;
    [SerializeField] CinemachineCamera vcamEf;

    [Header("Locators")]
    [SerializeField] GameObject vcamFLLocator;
    [SerializeField] GameObject vcamFCLocator;

    [Header("Default Camera Follow")]
    [SerializeField] private Transform vcamFollowAnchor;
    [SerializeField] private Transform followReference;   // 預設視角正對的那個點
    [SerializeField] private float followSmoothTime = 0.25f;

    [Header("Opening Animation")]
    [SerializeField] float openingDuration = 3f;

    [Header("Focus Settings")]
    [SerializeField] private Vector3 focusOffset = new Vector3(0, 2, -5);
    [SerializeField] private float focusTransitionDuration = 0.5f;


    [Inject] private SignalBus _signalBus;
    [Inject] private readonly IPieceViewRegistry _viewRegistry;
    [Inject] private readonly IBoardQueryService _boardQuery;

    // 使用 HashSet 儲存所有要求上鎖的來源 ID
    private HashSet<string> _lockTokens = new HashSet<string>();
    public bool IsLocked => _lockTokens.Count > 0;


    private CameraMode _currentMode = CameraMode.Uninitialized;
    private Transform _currentFocusTarget;
    private Transform _trackedPiece;
    private Vector3 _anchorHome;
    private Vector3 _anchorVelocity;
    private List<CinemachineCamera> _allCameras;
    private FreelookCameraController _freelookController;
    public CameraMode CurrentMode => _currentMode;
    private Coroutine _focusCoroutine;

    #region Lifecycle
    void Awake()
    {
        _allCameras = new List<CinemachineCamera> { vcamOp, vcam, vcamt, vcamf, vcamFL, vcamEf };
        foreach (var cam in _allCameras) cam.Priority = 0;
        TransitionTo(CameraMode.Default);
    }

    void Start()
    {
        _freelookController = vcamFLLocator.GetComponent<FreelookCameraController>();

        _signalBus.Subscribe<CameraFocusRequestSignal>(OnFocusRequested);
        _signalBus.Subscribe<CameraLockStateSignal>(OnCameraLockStateChanged);
        _signalBus.Subscribe<CameraModeRequestSignal>(OnCameraModeRequested);
        _signalBus.Subscribe<ResetCameraToDefaultSignal>(signal => RequestDefaultMode(true));

        _signalBus.Subscribe<MarkTargetPieceSignal>(OnTargetMarked);

        vcamf.Follow = vcamFCLocator.transform;

        var locator = vcam.Follow;
        vcamFollowAnchor.SetPositionAndRotation(locator.position, locator.rotation);
        _anchorHome = vcamFollowAnchor.position;
        vcam.Follow = vcamFollowAnchor;
    }

    void Update()
    {
        Vector3 goal = _anchorHome;
        if (_trackedPiece != null)
        {
            Vector3 d = _trackedPiece.position - followReference.position;
            goal += new Vector3(d.x, 0f, d.z);   // 只取 XZ 位移,Y 與旋轉完全不動
        }

        vcamFollowAnchor.position = Vector3.SmoothDamp(
            vcamFollowAnchor.position, goal, ref _anchorVelocity, followSmoothTime);
    }

    void OnDestroy()
    {
        _signalBus.Unsubscribe<CameraFocusRequestSignal>(OnFocusRequested);
        _signalBus.Unsubscribe<CameraLockStateSignal>(OnCameraLockStateChanged);
        _signalBus.Unsubscribe<CameraModeRequestSignal>(OnCameraModeRequested);
        _signalBus.Unsubscribe<ResetCameraToDefaultSignal>(signal => RequestDefaultMode(true));
        _signalBus.TryUnsubscribe<MarkTargetPieceSignal>(OnTargetMarked);
    }
    #endregion

    #region Core Logic
    private void TransitionTo(CameraMode newMode)
    {
        if (_currentMode == newMode) return;

        _currentMode = newMode;
        OnModeChanged(newMode);

        _signalBus.Fire(new CameraModeChangedSignal(newMode));
        Debug.Log($"Transitioned to {newMode} mode");
    }

    private void OnModeChanged(CameraMode mode)
    {
        if (_freelookController != null) _freelookController.enabled = (mode == CameraMode.Freelook);

        if (mode != CameraMode.Focus && _focusCoroutine != null)
        {
            StopCoroutine(_focusCoroutine);
            _focusCoroutine = null;
        }

        switch (mode)
        {
            case CameraMode.Opening:
                ApplyOpeningMode();
                break;
            case CameraMode.TopView:
                ApplyTopViewMode();
                break;
            case CameraMode.Default:
                ApplyDefaultMode();
                break;
            case CameraMode.Freelook:
                ApplyFreelookMode();
                break;
            case CameraMode.Focus:
                ApplyFocusMode();
                break;
            case CameraMode.FocusEnemy:
                ApplyFocusEnemyMode(); 
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

    private void OnTargetMarked(MarkTargetPieceSignal signal)
    {
        // 1. 向 Domain 層查詢這顆棋子的實體資料
        var piece = _boardQuery.GetPieceById(signal.TargetPieceId);
        if (piece == null) return;

        // 2. 只有當目標是白方（玩家自己的棋子）時，才讓 Default Camera 跟隨
        if (piece.Team == Team.White)
        {
            var view = _viewRegistry.GetView(signal.TargetPieceId);
            _trackedPiece = view != null ? view.transform : null;
            Debug.Log($"[CameraPresenter] Default Camera 鎖定並跟隨己方 TargetPiece: {signal.TargetPieceId}");
        }
        else
        {
            // 3. 若是黑方目標，可以印個 Log 確保邏輯有正確攔截
            _trackedPiece = null;
            Debug.Log($"[CameraPresenter] 目標為黑方棋子 {signal.TargetPieceId}，Default Camera 忽略跟隨。");
        }
    }

    private void SetOnlyActive(CinemachineCamera activeVcam, int priority = 10)
    {
        foreach (var cam in _allCameras)
        {
            cam.Priority = (cam == activeVcam) ? priority : 0;
        }
    }
    #endregion

    #region 業務實作
    private void ApplyOpeningMode()
    {
        SetOnlyActive(vcamOp, 50);
    }

    private void ApplyTopViewMode()
    {
        SetOnlyActive(vcamt, 25);
        Debug.Log("Switched to Top View Camera Mode");
    }

    private void ApplyDefaultMode()
    {
        SetOnlyActive(vcam, 10);
        vcamf.LookAt = null;
        vcamEf.LookAt = null;
        vcamEf.Follow = null;
        _currentFocusTarget = null;
        Debug.Log("Switched to Default Camera Mode");
    }

    private void ApplyFocusMode()
    {
        SetOnlyActive(vcamf, 20);

        if (_currentFocusTarget != null)
        {
            vcamf.LookAt = _currentFocusTarget;
            if (_focusCoroutine != null) StopCoroutine(_focusCoroutine);
            _focusCoroutine = StartCoroutine(MoveFocusLocatorToTarget(_currentFocusTarget));
        }
        Debug.Log("Switched to Focus Camera Mode");
    }

    private void ApplyFocusEnemyMode()
    {
        SetOnlyActive(vcamEf, 20);
        if (_currentFocusTarget != null)
        {
            vcamEf.Follow = _currentFocusTarget;
            vcamEf.LookAt = _currentFocusTarget;
        }
        Debug.Log("Switched to Focus Enemy Camera Mode");
    }

    private void ApplyFreelookMode()
    {
        SetOnlyActive(vcamFL, 15);
        Debug.Log("Switched to Freelook Camera Mode");
    }
    #endregion

    #region 外部委託
    public void RequestFreelookMode()
    {
        if (IsLocked) return;
        TransitionTo(CameraMode.Freelook);
    }

    public void RequestPieceFocusMode(Transform target)
    {
        if (IsLocked) return;
        _currentFocusTarget = target;
        TransitionTo(CameraMode.Focus);
    }

    public void RequestTopViewMode()
    {
        if (IsLocked) return;
        TransitionTo(CameraMode.TopView);
    }

    public void RequestDefaultMode(bool force = false)
    {
        if (!force && IsLocked) return;
        TransitionTo(CameraMode.Default);
    }

    public void RequestStandbyZoneMode()
    {
        if (IsLocked) return;
        // 目前沒有專屬攝影機，先切回預設
        TransitionTo(CameraMode.Default);
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
        TransitionTo(CameraMode.Opening); // 切換到 Opening 攝影機

        // 等待運鏡時間
        // (如果你有實作 openingStartPoint 到 EndPoint 的移動邏輯，也寫在這裡)
        yield return new WaitForSeconds(openingDuration);
        var dolly = vcamOp.GetComponent<CinemachineSplineDolly>();

        if (dolly != null)
        {
            float elapsed = 0f;
            while (elapsed < openingDuration)
            {
                elapsed += Time.deltaTime;
                // 將 Position 從 0 增加到 1 (假設使用 Normalized 模式)
                dolly.CameraPosition = elapsed / openingDuration;
                yield return null;
            }
            dolly.CameraPosition = 1f; // 確保結束時精確抵達終點
        }

        Debug.Log("[Camera] 運鏡結束，切換回預設視角...");
        TransitionTo(CameraMode.Default);

        // 通知 TutorialManager 開始教學！
        Debug.Log("[Camera] 發送 CameraOpeningFinishedSignal");
        _signalBus.Fire<CameraOpeningFinishedSignal>();
    }
    #endregion

    #region Focus Mode 運鏡邏輯 
    private System.Collections.IEnumerator MoveFocusLocatorToTarget(Transform target)
    {
        Vector3 targetPosition = target.position + new Vector3(0, 2, -5); // 目標前方偏上位置
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPosition = vcamFCLocator.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // 使用 SmoothStep 曲線
            t = t * t * (3f - 2f * t);
            vcamFCLocator.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        vcamFCLocator.transform.position = targetPosition;
    }
    #endregion

    #region Focus Enemy Mode 運鏡邏輯
    public void RequestEnemyFocusMode(Transform targetTransform)
    {
        if (targetTransform == null) return;

        _currentFocusTarget = targetTransform;
        TransitionTo(CameraMode.FocusEnemy);
    }
    #endregion
}

#region Signals
public struct CameraOpeningFinishedSignal { }

public struct CameraModeChangedSignal
{
    public CameraMode NewMode;

    public CameraModeChangedSignal(CameraMode newMode)
    {
        NewMode = newMode;
    }
}

public struct CameraLockStateSignal
{
    public string RequesterId;
    public bool IsLocked;
    public CameraLockStateSignal(string requesterId, bool isLocked)
    {
        RequesterId = requesterId;
        IsLocked = isLocked;
    }
}
#endregion