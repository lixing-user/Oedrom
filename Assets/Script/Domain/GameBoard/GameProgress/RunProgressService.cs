using System;
using UnityEngine;
using Zenject;

// Application/Services/RunProgressService.cs
public class RunProgressService : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly RunProgress _runProgress;

    public RunProgressService(SignalBus signalBus, RunProgress runProgress)
    {
        _signalBus = signalBus;
        _runProgress = runProgress;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<BattleResolveSignal>(OnBattleResolved);
    }

    public void Dispose()
    {
        _signalBus.Unsubscribe<BattleResolveSignal>(OnBattleResolved);
    }

    private void OnBattleResolved(BattleResolveSignal signal)
    {
        // 每當收到結算訊號，就增加次數
        _runProgress.RecordBattleCompleted();
    }
}
