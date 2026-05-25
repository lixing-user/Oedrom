using UnityEngine;
using Zenject;

public class GameEndState : IGameFlowState
{
    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;

    public GameEndState(GameFlowStateMachine flowMachine, DiContainer container)
    {
        _flowMachine = flowMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {

    }

    public void OnExit(GameFlowStateMachine machine)
    {

    }
}
