using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IMoveValidateService
{
    List<PiecePosition> GetValidMoves(ChessPiece piece);
    List<PiecePosition> GetMovesForPiece(PieceID pieceId);
    bool IsValidMove(PieceID pieceId, PiecePosition targetPos);
}


public class MoveValidator : IMoveValidateService
{
    private readonly GameBoard<Tile> _repository;
    public MoveValidator(GameBoard<Tile> repository)
    {
        _repository = repository;
    }


    public bool IsValidMove(PieceID pieceId, PiecePosition to)
    {
        var piece = _repository.GetPieceById(pieceId);
        if (piece == null || piece.IsCaptured)
            return false;

        // 委派給棋子的策略
        var validMoves = GetValidMoves(piece);

        return validMoves.Contains(to);
    }


    public List<PiecePosition> GetValidMoves(ChessPiece piece)
    {
        var strategy = piece.MovementStrategy;
        var moves = strategy.GetAvailableMove(piece, _repository);

        return moves;
    }


    public List<PiecePosition> GetMovesForPiece(PieceID pieceId)
    {
        var piece = _repository.GetPieceById(pieceId);
        return piece != null ? GetValidMoves(piece) : new List<PiecePosition>();
    }
}

