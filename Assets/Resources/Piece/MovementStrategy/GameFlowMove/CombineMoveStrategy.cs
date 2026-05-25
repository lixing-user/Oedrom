using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CompositeMoveStrategy : IMovementStrategy
{
    private readonly List<IMovementStrategy> _strategies;

    public CompositeMoveStrategy(IEnumerable<IMovementStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public List<PiecePosition> GetAvailableMove<TTile>(
        ChessPiece piece, 
        GameBoard<TTile> board,
        Func<PiecePosition, bool> isBlocked) where TTile : Tile
    {
        // Use a HashSet to avoid duplicate positions
        var allMoves = new HashSet<PiecePosition>();
        foreach (var strategy in _strategies)
        {
            // Get moves from each strategy
            var moves = strategy.GetAvailableMove(piece, board);
            foreach (var pos in moves) allMoves.Add(pos);
        }
        return allMoves.ToList();
    }
}

public interface IMovementStrategy
{
    List<PiecePosition> GetAvailableMove<TTile>(
        ChessPiece piece, 
        GameBoard<TTile> board,
        Func<PiecePosition, bool> isBlocked = null) where TTile : Tile;
}
