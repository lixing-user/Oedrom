using System.Collections.Generic;
using UnityEngine;

public interface IGameBoard
{
    BoardDimensions Dimensions { get; }
    void ClearBoard();
    bool MovePiece(PiecePosition from, PiecePosition to);
    void PlacePiece(ChessPiece piece, PiecePosition position);
    bool CapturePiece(PiecePosition position, ChessPiece capturer = null);
    bool RemovePiece(PiecePosition position);
    bool RestorePiece(PieceID pieceId, PiecePosition newPosition);
    bool SwapPieces(PiecePosition pos1, PiecePosition pos2);
    ChessPiece GetPieceAt(PiecePosition position);
    ChessPiece GetPieceById(PieceID id);
    IEnumerable<ChessPiece> GetAllPieces();
    IEnumerable<ChessPiece> GetAllPiecesOnBoard();
    IEnumerable<ChessPiece> GetPiecesByTeam(Team team);

    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearEvents();
}
