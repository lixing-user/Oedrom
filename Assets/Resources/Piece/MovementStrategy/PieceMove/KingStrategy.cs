using System;
using System.Collections.Generic;
using UnityEngine;

public class KingStrategy : IMovementStrategy
{
    private readonly BoardDimensions _board;
    public KingStrategy(BoardDimensions board)
    {
        _board = board;
    }

    private static readonly (int dx, int dy)[] KingOffsets = {
        ( 1, 0), ( 1, 1), (1, -1), ( 0, 1),
        ( 0, -1), ( -1, 1), (-1, 0), (-1, -1)
    };

    public List<PiecePosition> GetAvailableMove<TTile>(
        ChessPiece piece,
        GameBoard<TTile> board,
        Func<PiecePosition, bool> isBlocked) where TTile : Tile
    {
        var moves = new List<PiecePosition>();
        PiecePosition currentPos = piece.CurrentPosition;


        foreach (var (dx, dy) in KingOffsets)
        {
            var KingX = piece.CurrentPosition.X + dx;
            var KingY = piece.CurrentPosition.Y + dy;

            if (PiecePosition.TryCreate(KingX, KingY, _board, out PiecePosition targetPos))
            {
                var targetIni = board.GetPieceAt(targetPos);
                if (targetIni == null || piece.IsEnemyOf(targetIni))
                {
                    moves.Add(targetPos);
                }
            }
            if (PiecePosition.IsValidPosition(KingX, KingY, _board))
            {
                var pos = PiecePosition.Create(KingX, KingY, _board);

                var target = board.GetPieceAt(pos);
                if (target == null || piece.IsEnemyOf(target))
                    moves.Add(pos);
            }
        }
        return moves;
    }
}
