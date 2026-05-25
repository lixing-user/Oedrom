using System;
using System.Collections.Generic;
using UnityEngine;

// Application層的訊號：棋子移動執行指令
public class PieceMovedSignal
{
    public PieceID PieceId { get; }
    public PiecePosition FromPosition { get; } 
    public PiecePosition ToPosition { get; }
    public Vector3 From { get; }
    public Vector3 To { get; }
    public ChessPieceType PieceType { get; }
    public Team Team { get; }
    public BoardContextId ContextId { get; }

    public PieceMovedSignal(PieceID pieceId,
        PiecePosition fromPosition,
        PiecePosition toPosition,
        Vector3 from,
        Vector3 to,
        ChessPieceType pieceType,
        Team team,
        BoardContextId contextId)
    {
        PieceId = pieceId ?? throw new ArgumentNullException(nameof(pieceId));
        FromPosition = fromPosition;
        ToPosition = toPosition;
        From = from;
        To = to;
        PieceType = pieceType;
        Team = team;
        ContextId = contextId;
    }
}

public class MovesConfirmedSignal{}


public class PieceRestoredSignal
{
    public PieceID PieceId { get; }
    public PiecePosition Position { get; }
    public ChessPieceType PieceType { get; }
    public Team Team { get; }
    public BoardContextId ContextId { get; }

    public PieceRestoredSignal(
        PieceID pieceId,
        PiecePosition position,
        ChessPieceType type,
        Team team,
        BoardContextId contextId)
    {
        PieceId = pieceId;
        Position = position;
        PieceType = type;
        Team = team;
        ContextId = contextId;
    }
}


public class PiecesSwappedSignal
{
    public PieceID FirstPieceId { get; }
    public PieceID SecondPieceId { get; }
    public PiecePosition Position1 { get; }  // Piece1 的原位置（現在是 Piece2）
    public PiecePosition Position2 { get; }  // Piece2 的原位置（現在是 Piece1）
    public ChessPieceType Type1 { get; }
    public ChessPieceType Type2 { get; }
    public Team Team { get; }  // 同隊交換
    public BoardContextId ContextId { get; }

    public PiecesSwappedSignal(
        PieceID firstPieceId, 
        PieceID secondPieceId,
        PiecePosition position1,
        PiecePosition position2,
        ChessPieceType type1,
        ChessPieceType type2,
        Team team,
        BoardContextId contextId)
    {
        FirstPieceId = firstPieceId;
        SecondPieceId = secondPieceId;
        Position1 = position1;
        Position2 = position2;
        Type1 = type1;
        Type2 = type2;
        Team = team;
        ContextId = contextId;
    }
}


public struct PieceTeamChangedSignal
{
    public PieceID PieceId;
    public Team NewTeam;

    public PieceTeamChangedSignal(PieceID id, Team newTeam)
    {
        PieceId = id;
        NewTeam = newTeam;
    }
}