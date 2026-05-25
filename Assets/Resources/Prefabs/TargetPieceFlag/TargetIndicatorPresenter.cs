using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TargetIndicatorPresenter : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly IPieceViewRegistry _viewRegistry;
    private readonly TargetFlagView.Pool _flagPool;

    private Dictionary<PieceID, TargetFlagView> _activeFlags = new();

    public TargetIndicatorPresenter(SignalBus signalBus, IPieceViewRegistry registry, TargetFlagView.Pool flagPool)
    {
        _signalBus = signalBus;
        _viewRegistry = registry;
        _flagPool = flagPool;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<MarkTargetPieceSignal>(OnMarkTarget);
        _signalBus.Subscribe<PieceRemovedSignal>(OnPieceRemoved); // 監聽棋子死亡
    }

    public void Dispose()
    {
        _signalBus?.TryUnsubscribe<MarkTargetPieceSignal>(OnMarkTarget);
        _signalBus?.TryUnsubscribe<PieceRemovedSignal>(OnPieceRemoved);
    }


    private void OnMarkTarget(MarkTargetPieceSignal signal)
    {
        var view = _viewRegistry.GetView(signal.TargetPieceId);
        if (view == null) return;

        // 從水池拿一個旗標出來，並設定父物件為該棋子
        TargetFlagView flag = _flagPool.Spawn(view.transform);
        _activeFlags[signal.TargetPieceId] = flag;
    }

    private void OnPieceRemoved(PieceRemovedSignal signal)
    {
        // 如果被移除的棋子身上有旗標，將其回收
        if (_activeFlags.TryGetValue(signal.PieceId, out var flag))
        {
            flag.Despawn(_flagPool); // 呼叫旗標自己的 Despawn 動畫並回收
            _activeFlags.Remove(signal.PieceId);
        }
    }
}

public struct MarkTargetPieceSignal
{
    public PieceID TargetPieceId;
    public bool IsPrimary; // 是否為主要目標（例如國王）
}