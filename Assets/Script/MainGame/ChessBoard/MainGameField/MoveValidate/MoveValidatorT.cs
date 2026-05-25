using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;


// Infrastructure：移動驗證服務介面實作（第三步）
public class MoveValidatorT : IMoveValidateService
{
    [InjectOptional] private readonly ITutorialOverrideService _tutorialOverride;

    private readonly GameBoard<TerrainTile> _repository;
    private readonly ITerrainMoveRule _terrainSystem;
    public MoveValidatorT(GameBoard<TerrainTile> repository, ITerrainMoveRule terrainSystem)
    {
        _repository = repository;
        _terrainSystem = terrainSystem;
    }


    public bool IsValidMove(PieceID pieceId, PiecePosition to)
    {
        // === 1. 教學攔截 (防禦層) ===
        Vector2Int targetPos = new Vector2Int(to.X, to.Y);
        if (_tutorialOverride != null && _tutorialOverride.IsMoveRestricted(targetPos)) // 假設傳入座標
        {
            Debug.LogWarning($"[教學攔截] 不允許移動到 {to}，請照著教學指示操作。");
            return false;
        }

        // === 2. 原本的核心規則 ===
        var piece = _repository.GetPieceById(pieceId);
        if (piece == null || piece.IsCaptured)
            return false;

        // 委派給棋子的策略
        //var validMoves = GetValidMoves(piece);
        var validMoves = new HashSet<PiecePosition>( GetValidMoves(piece));

        Debug.Log($"[MoveValidator] 棋子: {piece.Type} at {piece.CurrentPosition}");
        Debug.Log($"[MoveValidator] 目標: {to}");
        Debug.Log($"[MoveValidator] 有效移動數: {validMoves.Count}");
        Debug.Log($"[MoveValidator] 有效移動列表: {string.Join(", ", validMoves)}");

        bool isValid = validMoves.Any(move => move.X == to.X && move.Y == to.Y);

        Debug.Log($"[MoveValidator] {to} 在列表中: {isValid}");

        return validMoves.Contains(to);
    }

    public List<PiecePosition> GetValidMoves(ChessPiece piece)
    {
        // 1. 從策略獲取基本移動
        var strategy = piece.MovementStrategy;
        var moves = strategy.GetAvailableMove(piece, _repository);

        // 2. 應用同隊過濾
        moves = moves.Where(pos =>
        {
            var targetPiece = _repository.GetPieceAt(pos);
            return targetPiece == null || targetPiece.IsEnemyOf(piece);
        }).ToList();

        // 3. 應用地形過濾
        if (_terrainSystem != null)
        {
            moves = moves.Where(pos =>
            {
                var targetTile = _repository.GetTileAt(pos);
                if (targetTile == null) return false;

                // 呼叫 TerrainMoveRule 來判斷
                return _terrainSystem.AllowsMove(targetTile.Terrain, piece.Type);
            }).ToList();
        }

        // === 4. 應用教學過濾 (UI 視覺層) ===
        // 將被教學系統禁止的格子從 List 中剔除，這樣 UI 就不會亮起錯誤的格子！
        if (_tutorialOverride != null)
        {
            moves.RemoveAll(pos => _tutorialOverride.IsMoveRestricted(new Vector2Int(pos.X, pos.Y)));
        }

        return moves;
    }

    public List<PiecePosition> GetMovesForPiece(PieceID pieceId)
    {
        var piece = _repository.GetPieceById(pieceId);
        return piece != null ? GetValidMoves(piece) : new List<PiecePosition>();
    }
}
