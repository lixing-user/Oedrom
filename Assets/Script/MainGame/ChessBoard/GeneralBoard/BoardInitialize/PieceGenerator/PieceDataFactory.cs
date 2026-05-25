using ChessPieceCardProduction;
using UnityEngine;
using Zenject;


public interface IPieceFactory
{
    ChessPiece CreatePiece( ChessPieceType type, Team team, PiecePosition position, CardID sourceCard = null);

    ChessPiece CreatePieceWithId(PieceID id, ChessPieceType type, Team team, PiecePosition pos);
    ChessPiece CreateStandbyPiece(ChessPieceType type, Team team, CardID sourceCard = null);
}

public class PieceFactory  : IPieceFactory
{
    private readonly IMovementStrategyFactory _strategyFactory;

    public PieceFactory(IMovementStrategyFactory strategyFactory)
    {
        _strategyFactory = strategyFactory;
    }


    public ChessPiece CreatePiece(
     ChessPieceType type,
     Team team,
     PiecePosition position,
     CardID sourceCard = null)
    {
        var strategy = _strategyFactory.Create(type);
        var pieceId = PieceID.Generate();

        return new ChessPiece(
            pieceId,
            sourceCard,
            type,
            team,
            position,
            strategy
        );
    }

    public ChessPiece CreatePieceWithId(
    PieceID id,
    ChessPieceType type,
    Team team,
    PiecePosition pos)
    {
        var strategy = _strategyFactory.Create(type);

        // 沿用傳入的 ID，不呼叫 PieceID.Generate()
        return new ChessPiece(
            id, 
            null, 
            type, 
            team, 
            pos, 
            strategy);
    }

    public ChessPiece CreateStandbyPiece(
        ChessPieceType type,
        Team team, 
        CardID sourceCard = null)
    {
        var strategy = _strategyFactory.Create(type);
        var pieceId = PieceID.Generate();

        // 傳入 null 作為位置，並明確指定初始狀態為 InStandby
        return new ChessPiece(
            pieceId,
            sourceCard,
            type,
            team,
            null,
            strategy,
            PieceState.InStandby,
            false
        );
    }
}

