using System;
using UnityEngine;
using Zenject;

public class TimerInterruptSupervisor : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly GameFlowStateMachine _flowMachine;

    public TimerInterruptSupervisor(SignalBus signalBus, GameFlowStateMachine flowMachine)
    {
        _signalBus = signalBus;
        _flowMachine = flowMachine;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<TimerExpiredSignal>(OnTimerExpired);
    }
    public void Dispose() 
    {
        _signalBus.TryUnsubscribe<TimerExpiredSignal>(OnTimerExpired);
    }

    private void OnTimerExpired()
    {
        GameFlow currentFlow = _flowMachine.GetCurrentFlow();

        // 如果已經在結算或結束，就不重複觸發
        if (currentFlow == GameFlow.Resolution || currentFlow == GameFlow.GameEnd)
            return;

        Debug.LogWarning("[Supervisor] 偵測到時間耗盡！強制狀態機進入結算階段。");
        _flowMachine.ChangeState(GameFlow.Resolution);
    }
}
