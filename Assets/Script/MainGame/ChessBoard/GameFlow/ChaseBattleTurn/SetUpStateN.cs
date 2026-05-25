using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

// 實作戰鬥開始前的設置、佈陣狀態
public class SetUpStateN : IGameFlowState
{
    private IMoveModeService _moveModeService;
   // private IBattleStateRestorer _stateRestorer;

    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;
    private  SignalBus _signalBus;
    private IGameBoard _gameBoard;
    private IMoveCounterService _moveCounter;
    private BattleContextHolder _battleContext;

    private IFlowFacade _flowFacade;


    public SetUpStateN(
        GameFlowStateMachine flowMachine, 
        DiContainer container)
    {
        _flowMachine = flowMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
        _moveModeService = _container.Resolve<IMoveModeService>();
        _gameBoard = _container.Resolve<IGameBoard>();
        _moveCounter = _container.Resolve<IMoveCounterService>();
       // _stateRestorer = _container.Resolve<IBattleStateRestorer>();
        _battleContext = _container.Resolve<BattleContextHolder>();

        _flowFacade = machine.GetFlowFacade();
        _signalBus = machine.GetSignalBus();

        Debug.Log("[Setup] 進入佈陣階段");

       // _stateRestorer.CaptureInitialStateIfNeeded();

        _moveModeService.SetMode(MoveExecutionMode.Direct);
        _flowFacade.DisableAllInput();

        int levelMaxMoves = 4;
        _moveCounter.ResetCounter(levelMaxMoves);

        _flowMachine.ChangeState(GameFlow.PlayerTurn);
    }

    public void OnExit(GameFlowStateMachine machine)
    {
        Debug.Log("[SetUpState] 離開佈陣階段");
    }
}
