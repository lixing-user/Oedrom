using UnityEngine;

public class PieceSpawnedSignal
{
    public PieceID PieceId { get; }
    public Vector3 To { get; }
    public ChessPieceType Type { get; }
    public Team Team { get; }
    public BoardContextId ContextId { get; }

    public PieceSpawnedSignal(PieceID pieceId, Vector3 to,
        ChessPieceType type, Team team, BoardContextId contextId)
    {
        PieceId = pieceId;
        To = to;
        Type = type;
        Team = team;
        ContextId = contextId;
    }
}

public class PieceRemovedSignal
{
    public PieceID PieceId { get; }
    public PiecePosition Position { get; }
    public ChessPieceType Type { get; }
    public Team Team { get; }
    public CaptureMode Mode { get; }
    public BoardContextId ContextId { get; }

    public PieceRemovedSignal(
        PieceID pieceId,
        PiecePosition position,
        ChessPieceType type,
        Team team,
        CaptureMode mode,
        BoardContextId contextId)
    {
        PieceId = pieceId;
        Position = position;
        Type = type;
        Team = team;
        Mode = mode;
        ContextId = contextId;
    }
}