using System.Linq;
using UnityEngine;
using Zenject;

public class ResolutionFlowStateS : IGameFlowState
{
    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;

    private SignalBus _signalBus;
    private IFlowFacade _flowFacade;
    private IGameBoard _gameBoard;
    private BattleContextHolder _battleContext;
    private IPieceDeploymentService _deploymentService;
    private IMoveCounterService _moveCounter;

    public ResolutionFlowStateS(
       GameFlowStateMachine flowMachine,
       DiContainer container)
    {
        _flowMachine = flowMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
        Debug.Log("[Resolution] 開始結算階段");

        _gameBoard = _container.Resolve<IGameBoard>();
        _battleContext = _container.Resolve<BattleContextHolder>();
        _deploymentService = _container.Resolve<IPieceDeploymentService>();
        _moveCounter = _container.Resolve<IMoveCounterService>();

        _flowFacade = machine.GetFlowFacade();
        _signalBus = machine.GetSignalBus();

        // 執行結算邏輯
        PerformResolution();
    }

    public void OnExit(GameFlowStateMachine machine)
    {
        Debug.Log("[Resolution] 結算完成");
    }

    #region 檢查條件
    private bool CheckVictoryConditions(out Team winnerTeam)
    {
        winnerTeam = Team.None;// 預設值

        var pending = _battleContext.CurrentBattle;
        if (pending == null)
        {
            Debug.Log("[ResolutionPhase] 無戰鬥上下文，跳過檢查");
            return false;
        }

        var attacker = _gameBoard.GetPieceById(pending.AttackerId);
        var defender = _gameBoard.GetPieceById(pending.DefenderId);

        Debug.Log($"[CheckPieces] AttackerId: {pending.AttackerId}");
        Debug.Log($"[CheckPieces] DefenderId: {pending.DefenderId}");
        Debug.Log($"[CheckPiece] attacker HashCode: {attacker?.GetHashCode()}");
        Debug.Log($"[CheckPiece] defender HashCode: {defender?.GetHashCode()}");

        //if (attacker == null || defender == null) return false;
        bool attackerDead = (attacker == null || attacker.IsCaptured);
        bool defenderDead = (defender == null || defender.IsCaptured);

        //if (attacker.IsCaptured || defender.IsCaptured)
        if (attackerDead || defenderDead)
        {
            string reason;
            if (attackerDead)
            {
                winnerTeam = defender != null ? defender.OriginalTeam : Team.White; // 攻擊方死，防守方贏
                reason = $"{pending.AttackerType} (攻擊方) 已被消滅";
            }
            else
            {
                winnerTeam = attacker != null ? attacker.OriginalTeam : Team.Black; // 防守方死，攻擊方贏
                reason = $"{pending.DefenderType} (防守方) 已被消滅";
            }

            Debug.Log($"[Victory] {winnerTeam} 勝出：{reason}");

            _battleContext.SetBattleResult(winnerTeam);
            _signalBus.Fire(new GameEndedSignal(winnerTeam, reason));

            return true;
        }

        return false;
    }
    #endregion

    #region 處理結果
    private void PerformResolution()
    {
        // 執行結算（檢查勝利條件等）
        bool gameEnded = CheckVictoryConditions(out Team winner);

        if (gameEnded)
        {
            HandleGameEndResources(winner);

            var battleType = _battleContext.CurrentBattle?.BattleType;
            if (winner == Team.White && (battleType != BattleType.Loot && battleType != BattleType.Event))
            {
                Debug.Log("[Resolution] 白方獲勝且非搶劫戰，觸發獎勵顯示");
                _signalBus.Fire<ShowRewardRequestedSignal>();

                if (!_moveCounter.IsExceeded())
                {
                    Debug.Log("[Resolution] 白方獲勝且『步數未耗盡』，解鎖特殊獎勵/追擊戰資格！");
                    _signalBus.Fire<ShowChaseBattleRequestedSignal>();
                }
                else
                {
                    Debug.Log("[Resolution] 白方獲勝但『步數已耗盡』，失去追擊戰資格。");
                }
            }

            _flowMachine.ChangeState(GameFlow.GameEnd);
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

    private void HandleGameEndResources(Team winner)
    {
        if (winner == Team.White)
        {
            Debug.Log("[Resolution] 白方勝出：完好回收場上所有棋子");
            _deploymentService.ClearPiecesAfterBattle(returnAsBroken: false);
        }
        else if (winner == Team.Black)
        {
            Debug.Log("[Resolution] 黑方勝出：銷毀備戰區，並以破損狀態回收場上所有棋子");
            _deploymentService.ClearPiecesAfterBattle(returnAsBroken: true);
        }
    }
    #endregion
}