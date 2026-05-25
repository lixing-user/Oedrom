using UnityEngine;
using Zenject;

public interface ITimerService
{
    float Duration { get; }
    float RemainingTime { get; }

    void ResetTimer(float duration);  // SetupFlow: Initialize(300f)
    void StartTimer();                // PlayerTurnFlow: OnGameStartSignal
    bool IsTimeUp();                  // 預防檢查
    void Pause();                     // 暫停功能
}

public class TimerService : ITimerService, ITickable
{
    [Inject] private SignalBus _signalBus;

    public float Duration { get; private set; }
    public float RemainingTime { get; private set; }
    private bool _isRunning;

    public void ResetTimer(float duration)
    {
        Duration = duration;
        RemainingTime = duration;
        _isRunning = false;
    }

    public void StartTimer()
    {
        _isRunning = true;
        _signalBus.Fire(new TimerStartedSignal(Duration));
    }

    public void Tick()
    {
        if (_isRunning)
        {
            RemainingTime -= Time.deltaTime;
            _signalBus.Fire(new TimerTickSignal(RemainingTime, Duration));

            if (RemainingTime <= 0)
            {
                RemainingTime = 0;
                _isRunning = false;
                _signalBus.Fire(new TimerExpiredSignal());
            }
        }
    }

    public void Pause() { _isRunning = false; }

    public bool IsTimeUp() => RemainingTime <= 0;
}

#region Signals
public class TimerStartedSignal
{
    public float Duration { get; }
    public TimerStartedSignal(float duration) => Duration = duration;
}

public class TimerTickSignal
{
    public float RemainingTime { get; }
    public float Duration { get; }
    public TimerTickSignal(float remainingTime, float duration)
    {
        RemainingTime = remainingTime;
        Duration = duration;
    }
}

public class TimerExpiredSignal { }
#endregion