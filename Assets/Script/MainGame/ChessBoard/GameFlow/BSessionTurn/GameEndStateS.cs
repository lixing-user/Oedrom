using UnityEngine;
using Zenject;

public class GameEndStateS : IGameFlowState
{
    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;

    private SignalBus _signalBus;
    private IFlowFacade _flowFacade;
    private IGameBoard _gameBoard;

    public GameEndStateS(GameFlowStateMachine gameFlowStateMachine, DiContainer container)
    {
        _flowMachine = gameFlowStateMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
        _gameBoard = _container.Resolve<IGameBoard>();

        _flowFacade = machine.GetFlowFacade();
        _signalBus = machine.GetSignalBus();

        Debug.Log("[GameEnd] 進入結束狀態：鎖定操作並開啟結算流程");

        _flowFacade.DisableAllInput();
    }

    public void OnExit(GameFlowStateMachine machine)
    {
        Debug.Log("[GameEnd] 離開結束狀態");

        _flowFacade.EnableAllInput();
    }
}
