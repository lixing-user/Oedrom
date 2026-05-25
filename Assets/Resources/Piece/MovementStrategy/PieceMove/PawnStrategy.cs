using System;
using System.Collections.Generic;
using UnityEngine;

public class PawnStrategy : IMovementStrategy
{
    private readonly BoardDimensions _board;
    public PawnStrategy(BoardDimensions board)
    {
        _board = board;
    }

    public List<PiecePosition> GetAvailableMove<TTile>(
        ChessPiece piece,
        GameBoard<TTile> board,
        Func<PiecePosition, bool> isBlocked) where TTile : Tile
    {
        var direction = (piece.Team == 0) ? 1 : -1;
        var moves = new List<PiecePosition>();

        // 檢查是否在棋盤內
        if (!piece.CurrentPosition.IsValid(_board))
            return moves;

        AddForwardMoves(moves, piece, board, direction);
        AddCaptureMoves(moves, piece, board, direction);

        return moves;
    }

    private void AddForwardMoves<TTile>(
        List<PiecePosition> moves, 
        ChessPiece piece,
        GameBoard<TTile> board, 
        int direction,
        Func<PiecePosition, bool> isBlocked = null) where TTile : Tile
    {
        int forward1X = piece.CurrentPosition.X;
        int forward1Y = piece.CurrentPosition.Y + direction;

        if (!IsValidCoordinate(forward1X, forward1Y))
            return;

        //One in front
        var forward1 = PiecePosition.Create(forward1X, forward1Y, _board);

        if (board.GetPieceAt(forward1) == null)
        {
            moves.Add(forward1);

            // 【關鍵判定】：如果第一格是會阻擋穿越的地形 (山岳/河流)
            // 那麼士兵就不能「穿越」它走到第二格，直接 return 中斷。
            if (isBlocked != null && isBlocked(forward1))
            {
                return;
            }

            //Two in front
            if (IsStartingPosition(piece))
            {
                int forward2X = forward1.X;
                int forward2Y = forward1.Y + direction;

                if (!IsValidCoordinate(forward2X, forward2Y))
                    return;

                var forward2 = PiecePosition.Create(forward2X, forward2Y, _board);
                if (board.GetPieceAt(forward2) == null)
                    moves.Add(forward2);
            }
        }
    }

    private void AddCaptureMoves<TTile>(
        List<PiecePosition> moves, 
        ChessPiece piece,
        GameBoard<TTile> board,
        int direction) where TTile : Tile
    {
        var diagonals = new[] { (-1, direction), (1, direction) }; // 左斜和右斜

        foreach (var (dx, dy) in diagonals)
        {
            int diagX = piece.CurrentPosition.X + dx;
            int diagY = piece.CurrentPosition.Y + dy;

            if (!IsValidCoordinate(diagX, diagY))
                continue;

            var diagPos = PiecePosition.Create(piece.CurrentPosition.X + dx, piece.CurrentPosition.Y + dy, _board);
            
            var targetPiece = board.GetPieceAt(diagPos);
            if (targetPiece != null && piece.IsEnemyOf(targetPiece))
            {
                moves.Add(diagPos);
            }
        }
    }

    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < _board.BoardSizeX && y >= 0 && y < _board.BoardSizeY;
    }

    private bool IsStartingPosition(ChessPiece piece) =>
        piece.CurrentPosition.Y == (piece.Team == 0 ? 1 : 6);
}
