using System;
using System.Collections.Generic;
using UnityEngine;

public class BishopStrategy : IMovementStrategy
{
    private readonly BoardDimensions _board;
    public BishopStrategy(BoardDimensions board)
    {
        _board = board;
    }

    public List<PiecePosition> GetAvailableMove<TTile>(
        ChessPiece piece,
        GameBoard<TTile> board,
        Func<PiecePosition, bool> isBlocked) where TTile : Tile
    {
        var moves = new List<PiecePosition>();
        var dimensions = board.Dimensions;

        // 檢查是否在棋盤內
        if (!piece.CurrentPosition.IsValid(_board))
            return moves;

        // 檢查四個對角線方向：右上、左上、右下、左下
        AddMovesInDirection(moves, piece, board, 1, 1, isBlocked);   // 右上
        AddMovesInDirection(moves, piece, board, -1, 1, isBlocked);  // 左上
        AddMovesInDirection(moves, piece, board, 1, -1, isBlocked);  // 右下
        AddMovesInDirection(moves, piece, board, -1, -1, isBlocked); // 左下

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
                // 因為規則中「主教」可以停留在山上，所以將這座山加入可移動清單。
                // 但不能繼續穿越到山後面的平原，因此中斷迴圈。
                moves.Add(pos);
                break;
            }

            var target =board.GetPieceAt(pos);

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