using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMovementStrategyFactory
{
    IMovementStrategy Create(ChessPieceType type);
}

public class MovementStrategyFactory : IMovementStrategyFactory
{
    private readonly BoardDimensions _board;

    // Zenject 自動注入 BoardDimensions
    public MovementStrategyFactory(BoardDimensions board)
    {
        _board = board;
    }

    public IMovementStrategy Create(ChessPieceType type)
    {
        return type switch
        {
            ChessPieceType.King => new KingStrategy(_board),
            ChessPieceType.Queen => new QueenStrategy(_board),
            ChessPieceType.Rook => new RookStrategy(_board),
            ChessPieceType.Bishop => new BishopStrategy(_board),
            ChessPieceType.Knight => new KnightStrategy(_board),
            ChessPieceType.Pawn => new PawnStrategy(_board),
            _ => throw new ArgumentException($"未支援的棋子類型: {type}")
        };
    }
}