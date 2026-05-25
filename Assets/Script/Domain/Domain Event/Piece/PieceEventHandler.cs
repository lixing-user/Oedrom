using System;
using UnityEngine;
using Zenject;

public class PieceEventHandler:
    IEventHandler<PieceMovedEvent>,
    IEventHandler<PieceSpawnedFromCardEvent>,
    IEventHandler<PieceCapturedEvent>,
    IEventHandler<PieceRestoredEvent>,
    IEventHandler<PiecesSwappedEvent>,
    IEventHandler<PieceTeamChangedEvent>,
    IInitializable,
    IDisposable
{
    private readonly IEventHandlerRegistry _registry;
    private readonly SignalBus _signalBus;
    private readonly ICoordinateConverter _coordinateConverter;
    private readonly BoardContextId _contextId;

    public PieceEventHandler(
        IEventHandlerRegistry registry,
        SignalBus signalBus,
        ICoordinateConverter coordinateConverter,
        BoardContextId contextId)
    {
        _registry = registry;
        _signalBus = signalBus;
        _coordinateConverter = coordinateConverter;
        _contextId = contextId;
    }

    public void Initialize()
    {
        _registry.Register<PieceMovedEvent>(this);
        _registry.Register<PieceSpawnedFromCardEvent>(this);
        _registry.Register<PieceCapturedEvent>(this);
        _registry.Register<PieceRestoredEvent>(this);
        _registry.Register<PiecesSwappedEvent>(this);
        _registry.Register<PieceTeamChangedEvent>(this);
        Debug.Log("[PieceHandler] 場景處理器已註冊");
    }

    public void Dispose()
    {
        _registry.Unregister<PieceMovedEvent>(this);
        _registry.Unregister<PieceSpawnedFromCardEvent>(this);
        _registry.Unregister<PieceCapturedEvent>(this);
        _registry.Unregister<PieceRestoredEvent>(this);
        _registry.Unregister<PiecesSwappedEvent>(this);
        _registry.Unregister<PieceTeamChangedEvent>(this) ;
        Debug.Log("[PieceHandler] 場景處理器已移除");
    }



    public void Handle(PieceMovedEvent @event)
    {
        if (@event.ContextId != _contextId) return;

        Vector3 fromWorldPos = _coordinateConverter.PieceToWorldPosition(@event.From);
        Vector3 toWorldPos = _coordinateConverter.PieceToWorldPosition(@event.To);

        _signalBus.Fire(new PieceMovedSignal(
            @event.PieceId,
            @event.From,
            @event.To,
            fromWorldPos,
            toWorldPos,
            @event.PieceType,
            @event.Team,
            @event.ContextId
        ));

        Debug.Log($"[PieceHandler] 棋子移動: {@event.PieceId} -> {toWorldPos}");
    }

    public void Handle(PieceSpawnedFromCardEvent @event)
    {
        if (@event.ContextId != _contextId) return;

        Vector3 spawnWorldPos = _coordinateConverter.PieceToWorldPosition(@event.Position);
        _signalBus.Fire(new PieceSpawnedSignal(
            @event.PieceId,
            spawnWorldPos,
            @event.PieceType,
            @event.Team,
            @event.ContextId
        ));
        Debug.Log($"[PieceHandler] 棋子從卡牌生成: {@event.PieceId} at {spawnWorldPos}");
    }

    public void Handle(PieceCapturedEvent @event)
    {
        if (@event.ContextId != _contextId)  return;

        _signalBus.Fire(new PieceRemovedSignal(
            @event.PieceId,
            @event.Position,
            @event.PieceType,
            @event.Team,
            @event.Mode,
            @event.ContextId
        ));
        Debug.Log($"[PieceHandler] 棋子被捕獲: {@event.PieceId}");
    }

    public void Handle(PieceTeamChangedEvent @event)
    {
        if (@event.ContextId != _contextId) return;

        _signalBus.Fire(new PieceTeamChangedSignal(
            @event.PieceId,
            @event.NewTeam
        ));
        Debug.Log($"[PieceHandler] 棋子轉換陣營: {@event.PieceId} ({@event.OldTeam} -> {@event.NewTeam})");
    }

    public void Handle(PieceRestoredEvent @event)
    {
        if (@event.ContextId != _contextId) return;

        Vector3 restoreWorldPos = _coordinateConverter.PieceToWorldPosition(@event.Position);
        _signalBus.Fire(new PieceRestoredSignal(
            @event.PieceId,
            @event.Position,
            @event.Type,
            @event.Team,
            @event.ContextId
        ));
        Debug.Log($"[PieceHandler] 棋子被復原: {@event.PieceId} at {restoreWorldPos}");
    }

    public void Handle(PiecesSwappedEvent @event)
    {
        if (@event.ContextId != _contextId) return;

        Vector3 firstWorldPos = _coordinateConverter.PieceToWorldPosition(@event.Position1);
        Vector3 secondWorldPos = _coordinateConverter.PieceToWorldPosition(@event.Position2);
        _signalBus.Fire(new PiecesSwappedSignal(
            @event.Piece1Id,
            @event.Piece2Id,
            @event.Position1,
            @event.Position2,
            @event.Type1,
            @event.Type2,
            @event.Team,
            @event.ContextId
        ));
        Debug.Log($"[PieceHandler] 棋子交換: {@event.Piece1Id} <-> {@event.Piece2Id}");
    }
}
