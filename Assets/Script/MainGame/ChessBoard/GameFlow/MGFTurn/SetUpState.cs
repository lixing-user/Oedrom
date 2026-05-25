using UnityEngine;
using Zenject;

// 實作遊戲開始前、場景切回後的劇情播放
public class SetUpState : IGameFlowState
{
    private readonly GameFlowStateMachine _flowMachine;
    private readonly DiContainer _container;
    private SignalBus _signalBus;

    //private OnboardingFlowManager _onboardingManager;
    private IMoveModeService _moveModeService;
    private ICameraPresenter _cameraPresenter;

    private bool _cutsceneCompleted = false;

    public SetUpState(GameFlowStateMachine flowMachine, DiContainer container)
    {
        _flowMachine = flowMachine;
        _container = container;
        _signalBus = container.Resolve<SignalBus>();
    }

    public void OnEnter(GameFlowStateMachine machine)
    {
        Debug.Log("[設置回合] 回合準備開始");

        //_onboardingManager = _container.TryResolve<OnboardingFlowManager>();
        _moveModeService = _container.Resolve<IMoveModeService>();
        _cameraPresenter = _container.Resolve<ICameraPresenter>();

        // 1. 訂閱運鏡結束信號
        _signalBus.Subscribe<CameraOpeningFinishedSignal>(OnCutsceneCompleted);

        // 2. 開始播放開場運鏡 (維持在 SetUpState)
        _cameraPresenter.PlayOpeningSequence();
    }

    private void OnCutsceneCompleted()
    {
        _cutsceneCompleted = true;
        _signalBus.Unsubscribe<CameraOpeningFinishedSignal>(OnCutsceneCompleted);
        Debug.Log("[Setup] 劇情播放完成，自動進入戰鬥");

        _moveModeService.SetMode(MoveExecutionMode.Preview);

        // 發送遊戲正式開始信號
        _signalBus.Fire(new GameStartedSignal(
            Team.White,
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        ));
        _signalBus.Fire(new ToggleHandDeckSignal());

        // 自動切換到玩家回合
        _flowMachine.ChangeState(GameFlow.PlayerTurn);

        //bool isTutorialActive = _onboardingManager != null && _onboardingManager.IsFlowActive;
        //if (isTutorialActive)
        //{
        //    Debug.Log("[Setup] 已進入 StandbyPhase，發送 StartTutorialSignal 觸發教學");
        //    _signalBus.Fire<StartTutorialSignal>();
        //}
    }

    public void OnExit(GameFlowStateMachine machine)
    {

    }
}
