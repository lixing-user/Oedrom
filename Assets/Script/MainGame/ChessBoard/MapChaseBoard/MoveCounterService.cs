using System;
using UnityEngine;
using Zenject;

public interface IMoveCounterService
{
    int MaxMoves { get;}
    int CurrentMoves { get;}
    void ResetCounter(int max);
    void Increment();
    bool CanMove();
    bool IsExceeded();
}

public class MoveCounterService : IMoveCounterService, IEventHandler<PieceMovedEvent>, IInitializable, IDisposable
{
    private readonly IEventHandlerRegistry _registry;
    private readonly SignalBus _signalBus;

    public int MaxMoves { get; private set; } = 3;  //  新手教學暫時設定為 Mate in 3
    public int CurrentMoves { get; private set; } = 0;

    #region Initialize
    public MoveCounterService(SignalBus signalBus, IEventHandlerRegistry registry)
    {
        _signalBus = signalBus;
        _registry = registry;
    }

    public void Initialize()        // 向 Dispatcher 註冊自己
    {
        _registry.Register<PieceMovedEvent>(this);
    }

    public void Dispose()
    {
        _registry.Unregister<PieceMovedEvent>(this);
    }
    #endregion

    #region 核心邏輯
    public void ResetCounter(int max)
    {
        MaxMoves = max;
        CurrentMoves = 0;
        _signalBus.Fire(new MoveCountChangedSignal(CurrentMoves, MaxMoves));
    }

    public void Handle(PieceMovedEvent @event)
    {
        // 只計算玩家（白方）的移動
        if (@event.Team == Team.White)
        {
            Increment();
        }
    }

    public void Increment()
    {
        CurrentMoves++;
        _signalBus.Fire(new MoveCountChangedSignal(CurrentMoves, MaxMoves));  // → UI動畫

        //if (CurrentMoves >= MaxMoves)
        //{
        //    _signalBus.Fire(new MoveLimitExceededSignal());  // → EndGameFlow
        //}
    }
    #endregion

    public bool CanMove() => CurrentMoves < MaxMoves;
    public bool IsExceeded() => CurrentMoves >= MaxMoves;
}

#region Signals
public class MoveCountChangedSignal
{
    public int Current { get; }
    public int Max { get; }
    public MoveCountChangedSignal(int current, int max)
        => (Current, Max) = (current, max);
}

public class MoveLimitExceededSignal { }
#endregion