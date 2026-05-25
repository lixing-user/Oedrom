using System;
using System.Collections.Generic;
using UnityEngine;

public class SetupPhaseMoveStrategy : IMovementStrategy
{
    private readonly BoardDimensions _board;
    private readonly int _setupRowLimit;

    public SetupPhaseMoveStrategy(BoardDimensions board, int setupRowLimit = 2)
    {
        _board = board;
        _setupRowLimit = setupRowLimit;
    }

    public List<PiecePosition> GetAvailableMove<TTile>(
      ChessPiece piece,
      GameBoard<TTile> board,
      Func<PiecePosition, bool> isBlocked) where TTile : Tile
    {
        var moves = new List<PiecePosition>();
        if (piece.Team != Team.White)
            return moves;

        // 只有在佈陣區域（底部兩排）的棋子才能移動
        if (piece.CurrentPosition == null || piece.CurrentPosition.Y >= _setupRowLimit)
            return moves;

        // 返回底部兩排的所有位置（包括空格和同隊棋子）
        for (int x = 0; x < _board.BoardSizeX; x++)
        {
            for (int y = 0; y < _setupRowLimit; y++)
            {
                var targetPos = PiecePosition.Create(x, y, _board);

                // 跳過自己當前的位置
                if (targetPos.Equals(piece.CurrentPosition))
                    continue;

                var targetPiece = board.GetPieceAt(targetPos);

                if (targetPiece == null || !piece.IsEnemyOf(targetPiece)) // 空格可以，友軍可以，敵軍不行
                {
                    moves.Add(targetPos);
                }
            }
        }


        return moves;
    }
}
