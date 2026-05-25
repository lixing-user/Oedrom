using System;
using UnityEngine;
using Zenject;

public class MGFContext : IInitializable, IDisposable
{
    [Inject] private SignalBus _signalBus;

    public Team? LastWinner { get; private set; }
    public string LastReason { get; private set; }

    public void Initialize()
    {
        _signalBus.Subscribe<GameEndedSignal>(OnGameEnded);
    }

    public void Dispose()
    {
        _signalBus?.TryUnsubscribe<GameEndedSignal>(OnGameEnded);
    }

    private void OnGameEnded(GameEndedSignal signal)
    {
        LastWinner = signal.WinnerTeam;
        LastReason = signal.Reason;
    }

    public void Clear()
    {
        LastWinner = null;
        LastReason = string.Empty;
    }
}
