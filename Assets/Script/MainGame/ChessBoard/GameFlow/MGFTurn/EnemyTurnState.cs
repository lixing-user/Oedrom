using UnityEngine;
using Zenject;

public class EnemyTurnState : IGameFlowState
{
    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;
    private SignalBus _signalBus;
    private GamePhaseStateMachine _phaseMachine;


    public EnemyTurnState(GameFlowStateMachine gameFlowStateMachine, DiContainer container)
    {
        _flowMachine = gameFlowStateMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
        Debug.Log("[EnemyTurnState] 進入敵方回合");

        _signalBus = machine.GetSignalBus();

        // 1. 取得子狀態機實例
        _phaseMachine = _container.Resolve<GamePhaseStateMachine>();

        // 2. 訂閱子狀態機全部完成的事件
        _phaseMachine.OnAllPhasesCompleted += HandleAllPhasesCompleted;

        // 3. 啟動子狀態機，從第一個階段 (Standby) 開始
        // 雖然黑方在 StandbyPhase 會自動跳過，但為了規則一致性，仍從頭開始
        _phaseMachine.StartPhases(GamePhaseState.StandbyPhase);
    }

    private void HandleAllPhasesCompleted()
    {
        Debug.Log("[EnemyTurnState] 黑方所有階段執行完畢，準備切換回玩家回合");

        // 切換回全域的處理狀態（或直接切換回 PlayerTurn，視您的 GameFlow 定義而定）
        // 建議切換到全域 Resolution 或直接 PlayerTurn
        _flowMachine.ChangeState(GameFlow.Resolution);
    }

    public void OnExit(GameFlowStateMachine machine)
    {
        Debug.Log("[EnemyTurnState] 離開敵方回合");

        if (_phaseMachine != null)
        {
            _phaseMachine.OnAllPhasesCompleted -= HandleAllPhasesCompleted;
            _phaseMachine.StopPhases();
        }
    }
}

public struct EnemyVisualMoveCompletedSignal { }