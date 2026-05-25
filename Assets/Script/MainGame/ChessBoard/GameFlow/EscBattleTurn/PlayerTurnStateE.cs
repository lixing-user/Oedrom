using System.Linq;
using UnityEngine;
using Zenject;

public class PlayerTurnStateE : IGameFlowState
{
    private IMoveModeService _moveModeService;

    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;
    private  SignalBus _signalBus;

    private IGameBoard _gameBoard;
    private BattleContextHolder _battleContext;

    private IFlowFacade _flowFacade;
    private bool _isPlayerActionComplete = false;


    public PlayerTurnStateE(
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
        _moveModeService = _container.Resolve<IMoveModeService>();
        _gameBoard = _container.Resolve<IGameBoard>();
        _battleContext = _container.Resolve<BattleContextHolder>();

        Debug.Log("[簡化回合] 玩家回合開始");

        _moveModeService.SetMode(MoveExecutionMode.Direct);

        _signalBus.Subscribe<PieceMovedSignal>(OnPieceMoved);

        _flowFacade.EnableAllInput();    // 允許所有操作
    }

    private void OnPieceMoved()
    {
        Debug.Log("[簡化回合] 玩家結束回合");
        _isPlayerActionComplete = true;
        _flowMachine.ChangeState(GameFlow.Resolution);
    }

    public void OnExit(GameFlowStateMachine machine)
    {
        _isPlayerActionComplete = false;
        _signalBus.Unsubscribe<PieceMovedSignal>(OnPieceMoved);
    }
}
