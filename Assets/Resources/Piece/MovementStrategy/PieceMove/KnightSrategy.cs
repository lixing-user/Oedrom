using System;
using System.Collections.Generic;
using UnityEngine;

public class KnightStrategy : IMovementStrategy
{
    private readonly BoardDimensions _board;
    public KnightStrategy(BoardDimensions board)
    {
        _board = board;
    }

    private static readonly (int dx, int dy)[] KnightOffsets = {
        ( 1,  2), ( 1, -2), (-1,  2), (-1, -2),
        ( 2,  1), ( 2, -1), (-2,  1), (-2, -1)
    };

    public List<PiecePosition> GetAvailableMove<TTile>(
        ChessPiece piece,
        GameBoard<TTile> board,
        Func<PiecePosition, bool> isBlocked) where TTile : Tile
    {
        var moves = new List<PiecePosition>();
        PiecePosition currentPos = piece.CurrentPosition;

        foreach (var (dx, dy) in KnightOffsets)
        {
            // 騎士L形移動
            var KnightX = piece.CurrentPosition.X + dx;
            var KnightY = piece.CurrentPosition.Y + dy;

            // 檢查是否在棋盤內
            if (PiecePosition.TryCreate(KnightX, KnightY, _board, out PiecePosition targetPos))
            {
                var targetIni = board.GetPieceAt(targetPos);
                if (targetIni == null || piece.IsEnemyOf(targetIni))
                {
                    moves.Add(targetPos);
                }
            }
            if (PiecePosition.IsValidPosition(KnightX, KnightY, _board))
            {
                var pos = PiecePosition.Create(KnightX, KnightY, _board);

                // 檢查目標位置\是否有敵方棋子或空位
                var target = board.GetPieceAt(pos);
                if (target == null || piece.IsEnemyOf(target))
                    moves.Add(pos);
            } 
        }
        return moves;
    }
}
