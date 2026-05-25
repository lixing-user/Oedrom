using UnityEngine;
using Zenject;

public class GameEndStateE : IGameFlowState
{
    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;

    public GameEndStateE(GameFlowStateMachine gameFlowStateMachine, DiContainer container)
    {
        _flowMachine = gameFlowStateMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {

    }

    public void OnExit(GameFlowStateMachine machine)
    {

    }
}
