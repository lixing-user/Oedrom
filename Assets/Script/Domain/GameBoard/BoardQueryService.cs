using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;


// Application/Board/IBoardQueryService.cs
public interface IBoardQueryService
{
    BoardDimensions Dimensions { get; }
    IEnumerable<ChessPiece> GetAllPieces();
    int GetPieceCountForTeam(Team team);
    IReadOnlyList<ChessPiece> GetPiecesForTeam(Team team);
    int GetBoardHeight();
    ChessPiece GetPieceAt(PiecePosition position);
    ChessPiece GetPieceAt(Vector2Int position);
    ChessPiece GetPieceById(PieceID id);
    IEnumerable<ChessPiece> GetPiecesByTeam(Team team);
    List<PiecePosition> GetAllEmptyPositions();
    PiecePosition GetRandomEmptyPosition();
}


public class BoardQueryService : IBoardQueryService
{
    private readonly GameBoard<Tile> _gameBoard;

    [Inject]
    public BoardQueryService(GameBoard<Tile> gameBoard)
    {
        _gameBoard = gameBoard;
    }

    public BoardDimensions Dimensions => _gameBoard.Dimensions;

    public IEnumerable<ChessPiece> GetAllPieces()
    {
        return _gameBoard.GetAllPieces();
    }

    public int GetPieceCountForTeam(Team team)
    {
        return _gameBoard.GetPieceCountByTeam(team);
    }

    public IReadOnlyList<ChessPiece> GetPiecesForTeam(Team team)
    {
        return _gameBoard.GetPiecesByTeam(team).ToList().AsReadOnly();
    }

    public int GetBoardHeight()
    {
        return _gameBoard.GetBoardHeight();
    }

    public ChessPiece GetPieceAt(PiecePosition position)
    {
        return _gameBoard.GetPieceAt(position);
    }

    public ChessPiece GetPieceAt(Vector2Int position)
    {
        return _gameBoard.GetPieceAt(new PiecePosition(position.x, position.y));
    }

    public ChessPiece GetPieceById(PieceID id)
    {
        return _gameBoard.GetPieceById(id);
    }

    public IEnumerable<ChessPiece> GetPiecesByTeam(Team team)
    {
        return _gameBoard.GetPiecesByTeam(team);
    }

    public List<PiecePosition> GetAllEmptyPositions()
    {
        return _gameBoard.GetAllEmptyPositions();
    }

    public PiecePosition GetRandomEmptyPosition()
    {
        var emptyPositions = _gameBoard.GetAllEmptyPositions();

        if (emptyPositions.Count == 0)
        {
            UnityEngine.Debug.LogWarning("[BoardQueryService] 棋盤已經滿了，找不到空位！");
            return null;
        }

        // 從空位中隨機挑選一個
        int randomIndex = UnityEngine.Random.Range(0, emptyPositions.Count);
        return emptyPositions[randomIndex];
    }
}


public class BoardQueryServiceT : IBoardQueryService
{
    private readonly GameBoard<TerrainTile> _gameBoard;

    [Inject]
    public BoardQueryServiceT(GameBoard<TerrainTile> gameBoard)
    {
        _gameBoard = gameBoard;
    }

    public BoardDimensions Dimensions => _gameBoard.Dimensions;

    public IEnumerable<ChessPiece> GetAllPieces()
    {
        return _gameBoard.GetAllPieces();
    }

    public int GetPieceCountForTeam(Team team)
    {
        return _gameBoard.GetPieceCountByTeam(team);
    }

    public IReadOnlyList<ChessPiece> GetPiecesForTeam(Team team)
    {
        return _gameBoard.GetPiecesByTeam(team).ToList().AsReadOnly();
    }

    public int GetBoardHeight()
    {
        return _gameBoard.GetBoardHeight();
    }

    public ChessPiece GetPieceAt(PiecePosition position)
    {
        return _gameBoard.GetPieceAt(position);
    }

    public ChessPiece GetPieceAt(Vector2Int position)
    {
        return _gameBoard.GetPieceAt(new PiecePosition(position.x, position.y));
    }

    public ChessPiece GetPieceById(PieceID id)
    {
        return _gameBoard.GetPieceById(id);
    }

    public IEnumerable<ChessPiece> GetPiecesByTeam(Team team)
    {
        return _gameBoard.GetPiecesByTeam(team);
    }

    public List<PiecePosition> GetAllEmptyPositions()
    {
        return _gameBoard.GetAllEmptyPositions();
    }

    public PiecePosition GetRandomEmptyPosition()
    {
        var emptyPositions = _gameBoard.GetAllEmptyPositions();

        if (emptyPositions.Count == 0)
        {
            UnityEngine.Debug.LogWarning("[BoardQueryService] 棋盤已經滿了，找不到空位！");
            return null;
        }

        // 從空位中隨機挑選一個
        int randomIndex = UnityEngine.Random.Range(0, emptyPositions.Count);
        return emptyPositions[randomIndex];
    }
}