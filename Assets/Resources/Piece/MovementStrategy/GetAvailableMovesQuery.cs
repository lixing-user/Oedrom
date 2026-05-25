using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

public class GetAvailableMovesQuery<TTile> where TTile : Tile
{
    private readonly GameBoard<TTile> _board;

    public GetAvailableMovesQuery(GameBoard<TTile> board)
    {
        _board = board;
    }


    public List<PiecePosition> GetTheoreticalMoves(PieceID pieceId)
    {
        var piece = _board.GetPieceById(pieceId);
        if (piece == null || piece.IsCaptured)
            return new List<PiecePosition>();

        var strategy = piece.MovementStrategy;

        return strategy.GetAvailableMove(piece, _board);
    }

    public List<PiecePosition> GetValidMoves(PieceID pieceId)
    {
        var piece = _board.GetPieceById(pieceId);
        if (piece == null || piece.IsCaptured)
            return new List<PiecePosition>();

        var moves = GetTheoreticalMoves(pieceId);

        // 過濾：不能移動到己方棋子位置
        moves = moves.Where(pos =>
        {
            var targetPiece = _board.GetPieceAt(pos);
            return targetPiece == null || targetPiece.IsEnemyOf(piece);
        }).ToList();

        return moves;
    }
}