using System.Collections.Generic;
using UnityEngine;
using Zenject;

// Application/BoardStateSnapshotProvider.cs
public class BoardStateSnapshotProvider : IBoardStateSnapshot
{
    [Inject] private readonly IBoardQueryService _queryService;
    [Inject] private readonly ITurnManager _turnManager;
    [Inject] private readonly BoardDimensions _dimensions;


    public BoardDimensions Dimensions => _dimensions;


    public ChessPiece GetPieceAt(PiecePosition position)
        => _queryService.GetPieceAt(position);


    public IEnumerable<ChessPiece> GetAllPieces()
        => _queryService.GetAllPieces();


    // み俱眖 GamePhaseModel 莉笆篈计沮
    public bool IsWhiteTurn => _turnManager.CurrentTurn == Team.White;


    // ˙计ノ 50 ˙キЫ砏玥ヘ玡倒 0
    public int HalfMoveClock => 0;


    // 计
    public int FullMoveNumber => _turnManager.TurnNumber > 0 ? _turnManager.TurnNumber : 1;
}

public interface IBoardStateSnapshot
{
    BoardDimensions Dimensions { get; }
    ChessPiece GetPieceAt(PiecePosition position);
    IEnumerable<ChessPiece> GetAllPieces();
    bool IsWhiteTurn { get; }
    int HalfMoveClock { get; }  // ˙计
    int FullMoveNumber { get; }  // ˙计
}