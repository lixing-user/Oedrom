using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

// 實作戰鬥開始前的設置、佈陣狀態
public class SetUpStateE : IGameFlowState
{
    private IMoveModeService _moveModeService;
    //private IBattleStateRestorer _stateRestorer;

    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;
    private  SignalBus _signalBus;
    private IGameBoard _gameBoard;
    private ITimerService _timerService;
    private BattleContextHolder _battleContext;

    private IFlowFacade _flowFacade;


    public SetUpStateE(
        GameFlowStateMachine flowMachine, 
        DiContainer container)
    {
        _flowMachine = flowMachine;
        _container = container;
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
       // _stateRestorer = _container.Resolve<IBattleStateRestorer>();
        _moveModeService = _container.Resolve<IMoveModeService>();
        _gameBoard = _container.Resolve<IGameBoard>();
        _battleContext = _container.Resolve<BattleContextHolder>();
        _timerService = _container.Resolve<ITimerService>();

        _flowFacade = machine.GetFlowFacade();
        _signalBus = machine.GetSignalBus();

        _signalBus.Subscribe<DeploymentConfirmedSignal>(OnDeploymentConfirmed);

        Debug.Log("[Setup] 進入佈陣階段");

        _moveModeService.SetMode(MoveExecutionMode.Direct);
        _flowFacade.DisableAllInput();

        // 設定這場遊戲的限制時間：3分鐘 (180秒)
        _timerService.ResetTimer(180f);
    }

    private void OnDeploymentConfirmed()
    {
        Debug.Log("[Setup] 玩家確認佈陣");

        _signalBus.Fire(new GameStartedSignal(
                Team.White,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            ));

        // 啟動計時器
        _timerService.StartTimer();

        _flowMachine.ChangeState(GameFlow.PlayerTurn);
    }

    public void OnExit(GameFlowStateMachine machine)
    {
        Debug.Log("[SetUpState] 離開佈陣階段");

        _signalBus.Unsubscribe<DeploymentConfirmedSignal>(OnDeploymentConfirmed);
    }
}
