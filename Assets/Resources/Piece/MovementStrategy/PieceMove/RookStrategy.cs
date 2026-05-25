using System;
using System.Collections.Generic;
using UnityEngine;

public class RookStrategy : IMovementStrategy
{
    private readonly BoardDimensions _board;
    public RookStrategy(BoardDimensions board)
    {
        _board = board;
    }

    public List<PiecePosition> GetAvailableMove<TTile>(
        ChessPiece piece,
        GameBoard<TTile> board,
        Func<PiecePosition, bool> isBlocked) where TTile : Tile
    {
        var moves = new List<PiecePosition>();

        // 檢查是否在棋盤內
        if (!piece.CurrentPosition.IsValid(_board))
            return moves;

        // 檢查四個方向：下、左、右、上
        AddMovesInDirection(moves, piece, board, 0, -1, isBlocked); // 下
        AddMovesInDirection(moves, piece, board, -1, 0, isBlocked); // 左
        AddMovesInDirection(moves, piece, board, 1, 0, isBlocked);  // 右
        AddMovesInDirection(moves, piece, board, 0, 1, isBlocked);  // 上

        return moves;
    }

    private void AddMovesInDirection<TTile>(
       List<PiecePosition> moves,
       ChessPiece piece,
       GameBoard<TTile> board,
       int dx,
       int dy,
       Func<PiecePosition, bool> isBlocked) where TTile : Tile
    {
        int x = piece.CurrentPosition.X + dx;
        int y = piece.CurrentPosition.Y + dy;

        while (x >= 0 && x < _board.BoardSizeX && y >= 0 && y < _board.BoardSizeY)
        {
            var pos = PiecePosition.Create(x, y, _board);

            if (isBlocked != null && isBlocked(pos))
            {
                break;
            }

            var target = board.GetPieceAt(pos);

            if (target == null)
            {
                moves.Add(pos);
            }
            else
            {
                if (piece.IsEnemyOf(target))
                    moves.Add(pos);
                break; // 遇到棋子（敵方或己方）後停止
            }
            x += dx;
            y += dy;
        }
    }
}