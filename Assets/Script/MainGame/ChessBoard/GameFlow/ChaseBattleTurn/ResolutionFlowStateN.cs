using System.Linq;
using UnityEngine;
using Zenject;

public class ResolutionFlowStateN : IGameFlowState
{
    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;

    private GameFlowModel _gameModel;
    private IBoardQueryService _boardQuery;
    private IMoveCounterService _moveCounter;
    private IFlowFacade _flowFacade;
    private SignalBus _signalBus;


    public ResolutionFlowStateN(
       GameFlowStateMachine flowMachine,
       DiContainer container)
    {
        _flowMachine = flowMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
        _boardQuery = _container.Resolve<IBoardQueryService>();
        _moveCounter = _container.Resolve<IMoveCounterService>();
        _gameModel = _container.TryResolve<GameFlowModel>();

        _signalBus = machine.GetSignalBus();
        _flowFacade = machine.GetFlowFacade();

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
        // 1. 檢查遊戲是否達到結束條件
        if (CheckGameEndConditions(out Team? winner, out string reason))
        {
            // 先切換狀態！讓狀態機確實進入 GameEnd
            _flowMachine.ChangeState(GameFlow.GameEnd);

            // 再發射信號！這時 Coordinator 接收信號並把狀態切回 Setup 就絕對安全了
            _signalBus.Fire(new GameEndedSignal(winner.Value, reason));

            return;
        }

        // 2. 遊戲繼續，進行回合切換
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
        // --- 優先判定 1：是否勝利？ ---
        // 條件：黑方（敵方）的國王是否被吃掉？(或者你未來可以換成真實的 Checkmate 判定)
        winner = null;
        reason = string.Empty;

        bool isEnemyKingDead = CheckIfKingIsDead(Team.Black);
        if (isEnemyKingDead || _gameModel.IsEnemyStuck)
        {
            winner = Team.White;
            reason = "[Resolution] 玩家在限制步數內擊敗敵方國王，挑戰成功！";
            return true;
        }

        if (_moveCounter.IsExceeded())
        {
            winner = Team.Black;
            reason = "[Resolution] 步數耗盡，未能擊敗對手，挑戰失敗！";
            return true;
        }

        return false;
    }

    // 查詢國王存活狀態
    private bool CheckIfKingIsDead(Team team)
    {
        var teamPieces = _boardQuery.GetPiecesForTeam(team);
        // 如果找不到該隊伍的 King，代表國王已陣亡
        bool hasKing = teamPieces.Any(p => p.Type == ChessPieceType.King);
        return !hasKing;
    }
}
