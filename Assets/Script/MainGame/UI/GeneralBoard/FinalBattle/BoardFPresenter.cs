using System;
using UnityEngine;
using Zenject;

public class BoardFPresenter : MonoBehaviour
{
    [Inject] private SignalBus _signalBus;
    [Inject] private BattleContextHolder _battleContextHolder;
    [Inject] private IPieceDeploymentService _deploymentService;

    private IBoardFView view;


    #region 初始化&綁定
    void Start()
    {
        view = GetComponent<IBoardFView>();

        _signalBus.Subscribe<PieceSpawnedSignal>(OnCardPlaced);
        _signalBus.Subscribe<ShowGameEndUISignal>(OnBattleEnded);
        BindViewEvents();

        view.ShowSetupPanel();
        RefreshDeploymentInfo();
    }
    void OnDestroy()
    {
        _signalBus.Unsubscribe<PieceSpawnedSignal>(OnCardPlaced);
        _signalBus.Unsubscribe<ShowGameEndUISignal>(OnBattleEnded);
        UnbindViewEvents();
    }

    private void BindViewEvents()
    {
        view.OnArrangeRequest += OnArrangePieceRequest;
        view.OnResetRequest += OnCleanupPieceRequest;
        view.OnStartGameClicked += OnStartGameRequest;
        view.OnBackMainGameRequest += OnBackToMGFRequest;
        view.OnBackClicked += OnBackClicked;
    }

    private void UnbindViewEvents()
    {
        view.OnArrangeRequest -= OnArrangePieceRequest;
        view.OnResetRequest -= OnCleanupPieceRequest;
        view.OnStartGameClicked -= OnStartGameRequest;
        view.OnBackMainGameRequest -= OnBackToMGFRequest;
        view.OnBackClicked -= OnBackClicked;
    }
    #endregion

    #region 效果實作
    private void OnArrangePieceRequest()
    {
        Debug.Log("[BoardNPresenter] 發送自動佈陣 Signal");
        _signalBus.Fire<ArrangePieceRequestedSignal>();

        RefreshDeploymentInfo();
    }

    private void OnCleanupPieceRequest()
    {
        Debug.Log("[BoardNPresenter] 發送清除棋子 Signal");
        _signalBus.Fire<CleanUpPieceRequestedSignal>();

        RefreshDeploymentInfo();
    }

    private void OnCardPlaced(PieceSpawnedSignal signal)
    {
        // 每當有棋子被放置時，刷新佈陣資訊
        RefreshDeploymentInfo();
    }

    private void OnStartGameRequest()
    {
        Debug.Log("[BoardNPresenter] 發送確認佈陣 Signal");
        _signalBus.Fire<DeploymentConfirmedSignal>();

        view.HideSetupPanel();
    }

    private void OnBattleEnded(ShowGameEndUISignal signal)
    {
        view.ShowMGFScenePanel();
    }

    private void OnBackToMGFRequest() 
    {
        Debug.Log("[BoardNPresenter] 發送切換場景 Signal");

        var sessionData = _battleContextHolder.CurrentBattle;
        if (sessionData == null)
        {
            Debug.LogError("[BoardNPresenter] 缺少戰鬥上下文！");
            return;
        }

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
                WinnerTeam = (int)_battleContextHolder.BattleWinner.Value,
                BattleType = sessionData.BattleType
            });

        //_signalBus.Fire(new SceneReactivatedSignal("NormalBattle"));
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