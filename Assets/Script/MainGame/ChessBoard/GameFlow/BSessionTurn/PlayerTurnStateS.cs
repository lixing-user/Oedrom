using System.Linq;
using UnityEngine;
using Zenject;

public class PlayerTurnStateS : IGameFlowState
{
    private IMoveModeService _moveModeService;

    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;
    private  SignalBus _signalBus;

    private IGameBoard _gameBoard;
    private BattleContextHolder _battleContext;

    private IFlowFacade _flowFacade;
    private bool _isPlayerActionComplete = false;


    public PlayerTurnStateS(
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
        _signalBus.Subscribe<PieceSpawnedSignal>(OnCardPlaced);
        _signalBus.Subscribe<PieceRestoredSignal>(OnPieceRestored);

        _flowFacade.EnableAllInput();    // 允許所有操作

        UnlockBattlePieces();
    }

    private void OnPieceMoved(PieceMovedSignal signal)
    {
        Debug.Log("[簡化回合] 玩家結束回合");
        _isPlayerActionComplete = true;
        _flowMachine.ChangeState(GameFlow.Resolution);
    }
    private void OnCardPlaced(PieceSpawnedSignal signal)
    {
        Debug.Log("[簡化回合] 玩家結束回合");
        _isPlayerActionComplete = true;
        _flowMachine.ChangeState(GameFlow.Resolution);
    }
    private void OnPieceRestored(PieceRestoredSignal signal)
    {
        Debug.Log("[簡化回合] 玩家結束回合");
        _isPlayerActionComplete = true;
        _flowMachine.ChangeState(GameFlow.Resolution);
    }

    public void OnExit(GameFlowStateMachine machine)
    {
        _isPlayerActionComplete = false;
        _signalBus.Unsubscribe<PieceMovedSignal>(OnPieceMoved);
        _signalBus.Unsubscribe<PieceSpawnedSignal>(OnCardPlaced);
        _signalBus.Unsubscribe<PieceRestoredSignal>(OnPieceRestored);
    }

    private void UnlockBattlePieces()
    {
        var pending = _battleContext.CurrentBattle;
        if (pending == null) return;

        var attacker = _gameBoard.GetPieceById(pending.AttackerId);
        if (attacker == null)
            Debug.LogError($"[嚴重錯誤] 找不到 AttackerID: {pending.AttackerId}！是否因為回溯導致 ID 改變？");

        var defender = _gameBoard.GetPieceById(pending.DefenderId);
        if (defender == null)
            Debug.LogError($"[嚴重錯誤] 找不到 DefenderID: {pending.DefenderId}！是否因為回溯導致 ID 改變？");

        attacker?.Unlock();
        defender?.Unlock();

        Debug.Log($"[PlayerTurn] 解鎖 {pending.AttackerType} / {pending.DefenderType} 棋子");
    }
}
