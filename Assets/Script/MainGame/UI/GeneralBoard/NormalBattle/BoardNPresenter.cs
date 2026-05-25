using System;
using UnityEngine;
using Zenject;

public class BoardNPresenter : MonoBehaviour
{
    [Inject] private ITutorialOverrideService _overrideService;
    [Inject] private SignalBus _signalBus;
    [Inject] private BattleContextHolder _battleContextHolder;
    [Inject] private IPieceDeploymentService _deploymentService;

    private IBoardNView view;


    #region 初始化&綁定
    void Start()
    {
        view = GetComponent<IBoardNView>();

        _signalBus.Subscribe<PieceSpawnedSignal>(OnCardPlaced);
        _signalBus.Subscribe<ShowGameEndUISignal>(OnBattleEnded);
        _signalBus.Subscribe<ShowChaseBattleRequestedSignal>(OnShowCBSPanelRequested);
        _signalBus.Subscribe<BattleResetCompletedSignal>(OnBattleResetCompleted);

        if (_battleContextHolder.CurrentBattle == null)
        {
            Debug.Log("[BoardNPresenter] 偵測到無戰鬥上下文 (展演模式)，隱藏佈陣介面");
            view.HideSetupPanel();
        }
        else 
        {
            view.ShowSetupPanel();
            RefreshDeploymentInfo();
        }

        BindViewEvents();
    }
    void OnDestroy()
    {
        _signalBus.Unsubscribe<PieceSpawnedSignal>(OnCardPlaced);
        _signalBus.Unsubscribe<ShowGameEndUISignal>(OnBattleEnded);
        _signalBus.Unsubscribe<ShowChaseBattleRequestedSignal>(OnShowCBSPanelRequested);
        _signalBus.Unsubscribe<BattleResetCompletedSignal>(OnBattleResetCompleted);
        UnbindViewEvents();
    }

    private void BindViewEvents()
    {
        view.OnArrangeRequest += OnArrangePieceRequest;
        view.OnResetRequest += OnCleanupPieceRequest;
        view.OnStartGameClicked += OnStartGameRequest;
        //view.OnUndoClicked += OnUndoRequest;
        view.OnBackMainGameRequest += OnExitBattleRequest;
        view.OnChaseBattleRequest += OnEngageCBRequest;
        view.OnBackClicked += OnBackClicked;
    }

    private void UnbindViewEvents()
    {
        view.OnArrangeRequest -= OnArrangePieceRequest;
        view.OnResetRequest -= OnCleanupPieceRequest;
        view.OnStartGameClicked -= OnStartGameRequest;
        //view.OnUndoClicked -= OnUndoRequest;
        view.OnBackMainGameRequest -= OnExitBattleRequest;
        view.OnChaseBattleRequest -= OnEngageCBRequest;
        view.OnBackClicked -= OnBackClicked;
    }
    #endregion

    #region 效果實作
    private void OnArrangePieceRequest()
    {
        Debug.Log("[BoardNPresenter] 發送自動佈陣 Signal");
        _signalBus.Fire<ArrangePieceRequestedSignal>();

        // 刷新佈陣資訊
        RefreshDeploymentInfo();
    }

    private void OnCleanupPieceRequest()
    {
        Debug.Log("[BoardNPresenter] 發送清除棋子 Signal");
        _signalBus.Fire<CleanUpPieceRequestedSignal>();

        // 刷新佈陣資訊
        RefreshDeploymentInfo();
    }

    private void OnStartGameRequest()
    {
        Debug.Log("[BoardNPresenter] 發送確認佈陣 Signal");
        _signalBus.Fire<DeploymentConfirmedSignal>();
        view.HideSetupPanel();
    }

    private void OnCardPlaced(PieceSpawnedSignal signal)
    {
        // 每當有棋子被放置時，刷新佈陣資訊
        RefreshDeploymentInfo();
    }

    private void OnBattleEnded(ShowGameEndUISignal signal)
    {
        var sessionData = _battleContextHolder.CurrentBattle;
        if (sessionData == null) return;

        view.ShowMGFScenePanel();
        view.ShowMGFScenePanel();
    }

    private void OnBattleResetCompleted()
    {
        if (_battleContextHolder.CurrentBattle == null)
        {
            Debug.LogWarning("[BoardNPresenter] 展演模式下收到重置信號，忽略 UI 顯示");
            return;
        }

        Debug.Log("[BoardNPresenter] 收到重置完成信號，重新顯示佈陣介面");
        view.ShowSetupPanel();
        RefreshDeploymentInfo();
    }

    private void OnExitBattleRequest() 
    {
        Debug.Log("[BoardNPresenter] 發送切換場景 Signal");

        var sessionData = _battleContextHolder.CurrentBattle;
        if (sessionData == null)
        {
            Debug.LogError("[BoardNPresenter] 缺少戰鬥上下文！");
            return;
        }

        int winner = _battleContextHolder.BattleWinner.HasValue
            ? (int)_battleContextHolder.BattleWinner.Value
            : -1;

        _signalBus.Fire(new BattleResolveSignal(
            attackerId: sessionData.AttackerId,
            defenderId: sessionData.DefenderId)
            {
                AttackerType = sessionData.AttackerType,
                DefenderType = sessionData.DefenderType,
                AttackerTeam = sessionData.AttackerTeam,
                DefenderTeam = sessionData.DefenderTeam,
                AttackerPos = sessionData.AttackerPos,
                DefenderPos = sessionData.DefenderPos,
                WinnerTeam = winner,
                BattleType = sessionData.BattleType
            });

        //_signalBus.Fire(new SceneReactivatedSignal("NormalBattle"));
    }

    private void OnShowCBSPanelRequested(ShowChaseBattleRequestedSignal signal)
    {
        view.ShowCBSScenePanel();
    }

    private void OnEngageCBRequest() 
    {
        Debug.Log("[BoardNPresenter] 發送切換場景 Signal");
        _signalBus.Fire(new ChangeBattleTypeSignal
        {
            NewBattleType = BattleType.Chase,
        });
    }

    private void OnBackClicked()
    {
        view.HideAllPanels();
    }
    #endregion

    private void RefreshDeploymentInfo()
    {
        // 1. 取得最新資料
        int currentCount = _deploymentService.GetDeployedPieceCount();
        int maxCount = _deploymentService.GetMaxDeploymentCount();

        // 2. 傳遞給 View 顯示
        view.UpdateDeploymentInfo(currentCount, maxCount);
    }
}

/// <summary>
/// 請求佈陣 Signal
/// </summary>
public class ArrangePieceRequestedSignal { }

/// <summary>
/// 請求清除 Signal
/// </summary>
public class CleanUpPieceRequestedSignal { }