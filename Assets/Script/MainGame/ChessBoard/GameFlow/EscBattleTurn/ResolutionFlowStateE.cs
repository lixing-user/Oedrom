using System.Linq;
using UnityEngine;
using Zenject;

public class ResolutionFlowStateE : IGameFlowState
{
    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;

    private SignalBus _signalBus;
    private ITimerService _timerService;
    private IBoardQueryService _boardQuery;
    private IFlowFacade _flowFacade;


    public ResolutionFlowStateE(
       GameFlowStateMachine flowMachine,
       DiContainer container)
    {
        _flowMachine = flowMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
        _flowFacade = machine.GetFlowFacade();
        _signalBus = machine.GetSignalBus();

        _boardQuery = _container.Resolve<IBoardQueryService>();
        _timerService = _container.Resolve<ITimerService>();

        Debug.Log("[Resolution] 開始結算階段");

        // 執行結算邏輯
        PerformResolution();
    }

    public void OnExit(GameFlowStateMachine machine)
    {
        Debug.Log("[Resolution] 結算完成");
    }

    private void PerformResolution()
    {
        // 執行結算（檢查勝利條件等）
        if (CheckGameEndConditions(out Team? winner, out string reason))
        {
            // 先強制狀態機切換到 GameEnd
            _flowMachine.ChangeState(GameFlow.GameEnd);

            // 狀態確立後，才發射信號。
            // 這樣當 Coordinator 攔截並切換回 Setup 時，就不會被上面的 ChangeState 覆蓋
            if (winner.HasValue)
            {
                //_signalBus.Fire(new CheckmateOccurredSignal(winner.Value, reason));
                _signalBus.Fire(new GameEndedSignal(winner.Value, reason));
            }
            return;
        }

        // 根據前一個狀態決定下一步
        GameFlow previousFlow = _flowMachine.GetPreviousFlow();

        switch (previousFlow)
        {
            case GameFlow.PlayerTurn:
                Debug.Log("[Resolution] 玩家回合結算完成 → 切換到敵方回合");
                _flowMachine.ChangeState(GameFlow.EnemyTurn);
                break;

            case GameFlow.EnemyTurn:
                Debug.Log("[Resolution] 敵方回合結算完成 → 切換到玩家回合");
                _flowMachine.ChangeState(GameFlow.PlayerTurn);
                break;

            default:
                Debug.LogWarning($"[Resolution] 未預期的前一狀態: {previousFlow}");
                _flowMachine.ChangeState(GameFlow.PlayerTurn);  // 預設回到玩家回合
                break;
        }
    }

    private bool CheckGameEndConditions(out Team? winner, out string reason)
    {
        winner = null;
        reason = string.Empty;

        var whitePieces = _boardQuery.GetPiecesForTeam(Team.White);

        // --- 最優先判定：玩家是否全軍覆沒？ ---
        if (!whitePieces.Any())
        {
            winner = Team.Black;
            reason = "[Resolution] 玩家全軍覆沒，無棋子可操作，挑戰失敗！";
            Debug.Log(reason);
            return true;
        }

        // --- 優先判定 1：是否時間耗盡失敗？ ---
        if (_timerService.IsTimeUp())
        {
            winner = Team.Black;
            reason = "[Resolution] 時間耗盡，未能逃脫，挑戰失敗！";
            Debug.Log(reason);
            return true;
        }

        // --- 優先判定 2：是否達成逃脫勝利條件？ ---
        bool isQueenAlive = whitePieces.Any(p => p.Type == ChessPieceType.Queen && !p.IsCaptured);

        if (!isQueenAlive)
        {
            winner = Team.Black;
            reason = "[Resolution] 玩家皇后已被俘虜，逃脫失敗！";
            Debug.Log(reason);
            return true;
        }

        // --- 優先判定 3：是否達成逃脫勝利條件？ ---
        foreach (var escapePoint in EscapeBattleSetup.EscapePoint)
        {
            Vector2Int targetPos = new Vector2Int(escapePoint.X, escapePoint.Y);
            ChessPiece pieceAtTarget = _boardQuery.GetPieceAt(targetPos);

            if (pieceAtTarget != null &&
                pieceAtTarget.Team == Team.White &&
                pieceAtTarget.Type == ChessPieceType.Queen)
            {
                winner = Team.White;
                reason = $"[Resolution] 玩家皇后已抵達逃脫點 ({targetPos.x}, {targetPos.y})，挑戰成功！";
                Debug.Log(reason);
                return true;
            }
        }

        return false; // 遊戲繼續
    }
}
