using System;
using UnityEngine;
using Zenject;

public class GlobalFlowStateTracker : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;

    // 記住當前的 Flow 狀態。預設給一個安全的初始值
    public GameFlow CurrentFlow { get; private set; } = GameFlow.Setup;
    public int CurrentTurnNumber { get; private set; } = 0;

    public GlobalFlowStateTracker(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void Initialize()
    {
        // 聽取來自局部場景的信號
        _signalBus.Subscribe<FlowChangedSignal>(OnFlowChanged);
        _signalBus.Subscribe<GlobalTurnChangedSignal>(OnGlobalTurnChanged);
    }

    public void Dispose()
    {
        _signalBus?.TryUnsubscribe<FlowChangedSignal>(OnFlowChanged);
        _signalBus?.TryUnsubscribe<GlobalTurnChangedSignal>(OnGlobalTurnChanged);
    }

    private void OnFlowChanged(FlowChangedSignal signal)
    {
        CurrentFlow = signal.CurrentFlow;
    }

    private void OnGlobalTurnChanged(GlobalTurnChangedSignal signal)
    {
        CurrentTurnNumber = signal.TurnNumber;
    }
}
