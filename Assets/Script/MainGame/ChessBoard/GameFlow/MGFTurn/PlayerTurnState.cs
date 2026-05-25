using System.Collections;
using UnityEngine;
using Zenject;


// 第二層 : 玩家回合狀態
public class PlayerTurnState : IGameFlowState
{
    private GamePhaseStateMachine _phaseMachine;
    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;


    public PlayerTurnState(GameFlowStateMachine gameFlowStateMachine, DiContainer container)
    {
        _flowMachine = gameFlowStateMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
        _phaseMachine = _container.TryResolve<GamePhaseStateMachine>();
        if (_phaseMachine == null)
        {
            Debug.LogError("[PlayerTurnState] GamePhaseStateMachine 未找到！請確認 MainGameField 場景已載入");
            return;
        }

        _phaseMachine.OnAllPhasesCompleted += HandleTurnEnd;

        // 從 StandbyPhase 開始
        _phaseMachine.StartPhases(GamePhaseState.StandbyPhase);
    }

    private void HandleTurnEnd()
    {
        // 玩家回合所有階段完成，切換到敵方回合
        _flowMachine.ChangeState(GameFlow.Resolution);
    }

    public void OnExit(GameFlowStateMachine machine)
    {
        _phaseMachine.OnAllPhasesCompleted -= HandleTurnEnd;
        _phaseMachine.StopPhases();
    }
}

