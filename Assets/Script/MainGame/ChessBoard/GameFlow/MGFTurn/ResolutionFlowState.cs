using UnityEngine;
using Zenject;

public class ResolutionFlowState : IGameFlowState
{
    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;
    private SignalBus _signalBus;
    private GamePhaseModel _phaseModel;
    private IResourceManager _resourceManager;
    private IVictoryCheckService _victoryChecker;
    private IBoardQueryService _boardQuery;

    public ResolutionFlowState(GameFlowStateMachine gameFlowStateMachine, DiContainer container)
    {
        _flowMachine = gameFlowStateMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
        Debug.Log("[ResolutionFlowState] 進入結算階段");

        _signalBus = machine.GetSignalBus();
        _phaseModel = _container.TryResolve<GamePhaseModel>();
        _resourceManager = _container.TryResolve<IResourceManager>();
        _victoryChecker = _container.TryResolve<IVictoryCheckService>();
        _boardQuery = _container.TryResolve<IBoardQueryService>();


        Debug.Log("[ResolutionFlowState] 等待結算結果...");

        // 1. 執行棋盤上所有棋子的狀態扣減 (實體狀態：瘟疫、暈眩)
        ProcessAllPiecesStatuses();

        // 1.5 執行全域資源/Buff的狀態扣減 (全域狀態：補給中斷、特定權重提升)
        ProcessGlobalModifiers();

        // 2. 處理勝利條件判斷
        EvaluateAndRoute();
    }



    public void OnExit(GameFlowStateMachine machine)
    {
        Debug.Log("[ResolutionFlowState] 退出結算階段");

        // 取消訂閱
        //if (_signalBus != null)
        //{
        //    _signalBus.TryUnsubscribe<BattleResultSignal>(OnBattleResult);
        //    _signalBus.TryUnsubscribe<EventResolutionSignal>(OnEventResolution);
        //    _signalBus = null;
        //}
    }


    /// <summary>
    /// 統一處理所有棋子的持續性狀態
    /// </summary>
    private void ProcessAllPiecesStatuses()
    {
        if (_boardQuery == null) return;

        var allPieces = _boardQuery.GetAllPieces();
        foreach (var piece in allPieces)
        {
            piece.TickStatuses();
        }

        // 發送 Signal 通知 UI (PieceInfoView) 更新目前的狀態圖示
        // _signalBus.Fire(new PieceStatusUpdatedSignal()); 
    }


    private void ProcessGlobalModifiers()
    {
        if (_resourceManager != null)
        {
            // 呼叫你在 IResourceManager 中定義好的方法
            // 來扣減那些 duration > 1 的 ResourceModifier
        }
    }


    private void EvaluateAndRoute()
    {
        // 1. 檢查勝利條件
        if (_victoryChecker != null)
        {
            var result = _victoryChecker.CheckVictoryConditions();
            if (result.IsGameOver)
            {
                Debug.Log($"[ResolutionFlowState] 遊戲結束！獲勝者: {result.Winner}, 原因: {result.Reason}");

                // 可以發送一個全局信號讓 UI 顯示結算畫面
                _signalBus.Fire(new GameEndedSignal(result.Winner.Value, result.Reason));

                // 將狀態機切換到 GameEndState，終止回合循環
                _flowMachine.ChangeState(GameFlow.GameEnd);
                return;
            }
        }

        // 2. 如果遊戲還沒結束，才決定下一個回合是誰
        DetermineNextFlow();
    }

    private void DetermineNextFlow()
    {
        // 假設你的 GameFlowStateMachine 已經有公開 GetPreviousFlow() 方法
        GameFlow previousFlow = _flowMachine.GetPreviousFlow();

        switch (previousFlow)
        {
            case GameFlow.PlayerTurn:
                Debug.Log("[ResolutionFlowState] 玩家回合結算完成 → 輪到敵方(AI)回合");
                _flowMachine.ChangeState(GameFlow.EnemyTurn);
                break;

            case GameFlow.EnemyTurn:
                Debug.Log("[ResolutionFlowState] 敵方回合結算完成 → 輪到玩家回合");
                _flowMachine.ChangeState(GameFlow.PlayerTurn);
                break;

            case GameFlow.Setup:
                Debug.Log("[ResolutionFlowState] 佈署階段結算完成 → 遊戲開始 (玩家先手)");
                _flowMachine.ChangeState(GameFlow.PlayerTurn);
                break;

            default:
                Debug.LogWarning($"[ResolutionFlowState] 未預期的前一狀態: {previousFlow}，預設切回玩家回合");
                _flowMachine.ChangeState(GameFlow.PlayerTurn);
                break;
        }
    }

    private void HandleTurnEnd()
    {
        Debug.Log("[ResolutionFlowState] 收到結算完成信號");

        // 清空標記，避免無限迴圈
        if (_phaseModel != null)
        {
            _phaseModel.IsBattleTriggered = false;
            _phaseModel.IsRandomEventTriggered = false;
            _phaseModel.CurrentResolutionType = ResolutionType.None;
        }

        DetermineNextFlow();
    }
}
