using UnityEngine;
using Zenject;

public class BoardEPresenter : MonoBehaviour
{
    [Inject] private ITutorialOverrideService _overrideService;
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly ITimerService _timerService;

    private BoardEView _view;

    void Start()
    {
        _view = GetComponent<BoardEView>();

        _signalBus.Subscribe<TimerTickSignal>(OnTimerTick);
        _signalBus.Subscribe<TimerExpiredSignal>(OnTimerExpired);
        _signalBus.Subscribe<BattleResetCompletedSignal>(OnBattleResetCompleted);

        BindViewEvent();
        InitializeUIFromService();
    }
    private void OnDestroy()
    {
        _signalBus.TryUnsubscribe<TimerTickSignal>(OnTimerTick);
        _signalBus.TryUnsubscribe<TimerExpiredSignal>(OnTimerExpired);
        _signalBus.TryUnsubscribe<BattleResetCompletedSignal>(OnBattleResetCompleted);

        UnbindViewEvent();
    }

    private void BindViewEvent()
    {
        _view.OnStartGameClicked += OnStartGameRequest;
        _view.OnEndGameRequest += OnEndGameRequested;
    }
    private void UnbindViewEvent()
    {
        _view.OnStartGameClicked -= OnStartGameRequest;
        _view.OnEndGameRequest -= OnEndGameRequested;
    }

    #region Timer Event Handlers
    private void InitializeUIFromService()
    {
        if (_timerService == null) return;

        // 初始化 UI (例如生成計時條)
        _view.InitializeTimerBar();

        // 顯示當前進度
        _view.UpdateTimerDisplay(_timerService.RemainingTime, _timerService.Duration);

        _view.ShowSetupPanel();
    }

    private void OnTimerTick(TimerTickSignal signal)
    {
        // 告訴 View 更新計時條比例
        _view.UpdateTimerDisplay(signal.RemainingTime, signal.Duration);
    }

    private void OnTimerExpired()
    {
        // 可以讓 View 播放時間到的特效 (例如閃爍紅色)
        _view.PlayTimeUpEffect();
    }
    #endregion

    #region View Event Handlers
    private void OnStartGameRequest()
    {
        Debug.Log("[BoardEPresenter] 開始遊戲按鈕被點擊，發送開始遊戲的 Signal");
        // 當玩家點擊開始遊戲按鈕時，發送開始遊戲的 Signal
        _signalBus.Fire<DeploymentConfirmedSignal>();
        _view.HideSetupPanel();
    }

    private void OnEndGameRequested()
    {
        // 當玩家點擊結束遊戲按鈕時，發送結束遊戲的 Signal
        //_signalBus.Fire<EndGameRequestedSignal>();
    }

    private void OnBattleResetCompleted()
    {
        Debug.Log("[BoardNPresenter] 收到重置完成信號，重新顯示佈陣介面");
        _view.ShowSetupPanel();
    }
    #endregion
}
