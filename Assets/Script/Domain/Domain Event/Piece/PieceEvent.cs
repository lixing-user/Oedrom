using ChessPieceCardProduction;
using UnityEngine;

public enum BoardContextId
{
    MainGameField, // 大世界
    BattleBoard    // 所有戰鬥共用的標籤
}

#region General Piece Events
public class PieceMovedEvent : DomainEvent
{
    public PieceID PieceId { get; }
    public PiecePosition From { get; }
    public PiecePosition To { get; }
    public ChessPieceType PieceType { get; }
    public Team Team { get; }
    public BoardContextId ContextId { get; }

    public PieceMovedEvent(
        PieceID pieceId, 
        PiecePosition from, 
        PiecePosition to, 
        ChessPieceType type, 
        Team team,
        BoardContextId contextId)
    {
        PieceId = pieceId;
        From = from;
        To = to;
        PieceType = type;
        ContextId = contextId;
        Team = team;
    }
}


public class PieceSpawnedFromCardEvent : DomainEvent
{
    public PieceID PieceId { get; }
    public CardID SourceCardId { get; }
    public ChessPieceType PieceType { get; }
    public PiecePosition Position { get; }
    public Team Team { get; }
    public BoardContextId ContextId { get; }

    public PieceSpawnedFromCardEvent(
        PieceID pieceId,
        CardID sourceCardId,
        ChessPieceType type,
        PiecePosition pos,
        Team team,
        BoardContextId contextId)
    {
        PieceId = pieceId;
        SourceCardId = sourceCardId;
        PieceType = type;
        Position = pos;
        ContextId = contextId;
        Team = team;
    }
}


public class PieceCapturedEvent : DomainEvent
{
    public PieceID PieceId { get; }
    public ChessPieceType PieceType { get; }
    public PiecePosition Position { get; }
    public Team Team { get; }
    public CaptureMode Mode { get; }
    public BoardContextId ContextId { get; } 

    public PieceCapturedEvent(
        PieceID pieceId,
        ChessPieceType type,
        PiecePosition position,
        Team team,
        BoardContextId contextId,
        CaptureMode mode = CaptureMode.ToStandby)
    {
        PieceId = pieceId;
        PieceType = type;
        Position = position;
        Team = team;
        ContextId = contextId;
        Mode = mode;
    }
}
#endregion

#region Battle Session Events
public class PiecesSwappedEvent : DomainEvent
{
    public PieceID Piece1Id { get; }
    public PieceID Piece2Id { get; }
    public PiecePosition Position1 { get; }  // Piece1 的原位置（現在是 Piece2）
    public PiecePosition Position2 { get; }  // Piece2 的原位置（現在是 Piece1）
    public ChessPieceType Type1 { get; }
    public ChessPieceType Type2 { get; }
    public Team Team { get; }  // 同隊交換
    public BoardContextId ContextId { get; }

    public PiecesSwappedEvent(
    PieceID piece1Id, PieceID piece2Id,
    PiecePosition position1, PiecePosition position2,
    ChessPieceType type1, ChessPieceType type2,
    Team team, BoardContextId contextId)
    {
        Piece1Id = piece1Id;
        Piece2Id = piece2Id;
        Position1 = position1;
        Position2 = position2;
        Type1 = type1;
        Type2 = type2;
        Team = team;
        ContextId = contextId;
    }

    public override string ToString() =>
    $"[交換] {Team}方的 {Type1}({Piece1Id.Value}) 與 {Type2}({Piece2Id.Value}) " +
    $"交換位置：{Position1} <swap> {Position2}";
}


public class PieceRestoredEvent : DomainEvent
{
    public PieceID PieceId { get; }
    public PiecePosition Position { get; }
    public ChessPieceType Type { get; }
    public Team Team { get; }
    public BoardContextId ContextId { get; }

    public PieceRestoredEvent(
        PieceID pieceId,
        PiecePosition position,
        ChessPieceType type,
        Team team,
        BoardContextId contextId)
    {
        PieceId = pieceId;
        Position = position;
        Type = type;
        Team = team;
        ContextId = contextId;
    }
}


public class PieceTeamChangedEvent : DomainEvent
{
    public PieceID PieceId { get; }
    public Team OldTeam { get; }
    public Team NewTeam { get; }
    public ChessPieceType PieceType { get; }
    public BoardContextId ContextId { get; }

    public PieceTeamChangedEvent(
        PieceID pieceId,
        Team oldTeam,
        Team newTeam,
        ChessPieceType type,
        BoardContextId contextId)
    {
        PieceId = pieceId;
        OldTeam = oldTeam;
        NewTeam = newTeam;
        PieceType = type;
        ContextId = contextId;
    }
}
#endregion