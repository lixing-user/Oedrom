using ChessPieceCardProduction;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

// 棋子管理子系統介面
public interface IPieceSpawner
{
    void SpawnSinglePiece(ChessPieceType type, PiecePosition pos, Team team);
    ChessPiece SpawnPieceFromCard(CardID cardId, ChessPieceType type, PiecePosition pos, Team team);
    ChessPiece SpawnPieceWithId(PieceID existingId, ChessPieceType type, PiecePosition pos, Team team);
    ChessPiece SpawnPieceToStandbyZone(ChessPieceType type, Team team);
    void RemovePiece(PieceID id);
    void RemoveAllPieces();
    IEnumerable<ChessPiece> GetChessPieces();
}



public class PieceSpawner : IPieceSpawner
{
    // Domain
    [Inject] private IGameBoard _gameBoard;
    [Inject] private IPieceFactory _pieceDataFactory;

    [InjectOptional] private IStandbyZone _standbyZone;
    [InjectOptional] private StandbyZoneService _standbyZoneService;

    // Infrastructure
    [Inject] private IBoardTileGenerator _tileGenerator;
    [Inject] private ITerrainHeightSampler _terrainSampler;
    [Inject] private BoardSettings _settings;
    [Inject] private ICoordinateConverter _coordinateConverter;

    // Presentation
    [Inject] private PieceView.Factory _piecePrefabFactory;  // View Factory
    [Inject] private IPieceViewRegistry _viewRegistry;

    private Dictionary<PieceID, PieceView> _pieceViews = new();


    #region 業務邏輯
    public IEnumerable<ChessPiece> GetChessPieces() => _gameBoard.GetAllPieces();

    public void SpawnSinglePiece(
        ChessPieceType type,
        PiecePosition pos, 
        Team team)
    {
        var piece = _pieceDataFactory.CreatePiece(type, team, pos);
        try
        {
            _gameBoard.PlacePiece(piece, pos);  
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogError($"無法放置棋子: {ex.Message}");
            return;
        }

        string prefabName = $"PiecePrefabs/{type}";  // Resource 名稱
        CreatePieceView(piece, team);
    }


    public ChessPiece SpawnPieceWithId(
    PieceID existingId,
    ChessPieceType type,
    PiecePosition pos,
    Team team)
    {
        if (existingId == null)
            throw new ArgumentNullException(nameof(existingId), "PieceID 不能為空");

        // 用指定 ID 創建棋子，不產生新的 GUID
        var piece = _pieceDataFactory.CreatePieceWithId(existingId, type, team, pos);

        try
        {
            _gameBoard.PlacePiece(piece, pos);
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogError($"無法放置棋子（指定ID）: {ex.Message}");
            return null;
        }

        CreatePieceView(piece, team);
        Debug.Log($"[PieceSpawner] SpawnPieceWithId: {type} ID={existingId}");
        return piece;
    }


    public ChessPiece SpawnPieceFromCard(
        CardID cardId, 
        ChessPieceType type, 
        PiecePosition pos, 
        Team team)
    {
        if (cardId == null)
            throw new System.ArgumentNullException(nameof(cardId), "卡牌 ID 不能為空");

        var piece = _pieceDataFactory.CreatePiece(type, team, pos, cardId);
        try
        {
            _gameBoard.PlacePiece(piece, pos);
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogError($"無法放置棋子: {ex.Message}");
            return null;
        }


        string prefabName = $"PiecePrefabs/{type}";  // Resource 名稱
        CreatePieceView(piece, team);

        return piece;
    }


    public ChessPiece SpawnPieceToStandbyZone(ChessPieceType type, Team team)
    {
        Debug.Log("1. PieceSpawner 準備加入棋子");

        // 1. 建立資料層棋子
        var piece = _pieceDataFactory.CreateStandbyPiece(type, team);

        Debug.Log($"3. 棋子狀態確認: {piece.State}");

        // 2. 註冊至待命區 Domain
        _standbyZone.AddPiece(piece);

        // 3. 建立視覺 (View)
        CreatePieceView(piece, team);

        // 4. 通知 StandbyZoneView 重新排列位置
        _standbyZoneService.RefreshLayout(team, StandbySource.Spawned, piece.Id);

        return piece;
    }


    public void RemovePiece(PieceID id)
    {
        if (!_pieceViews.ContainsKey(id))
        {
            Debug.LogWarning($"視圖不存在: {id}");
            return;
        }

        var view = _viewRegistry.GetView(id);
        if (view == null)
        {
            Debug.LogWarning($"[PieceSpawner] 視圖不存在: {id}");
            return;
        }
 
        var keyToRemove = _pieceViews.Keys.FirstOrDefault(k => k.Value == id.Value);
        if (keyToRemove != null)
            _pieceViews.Remove(keyToRemove);

        DestroyPieceView(id);
            Debug.Log($"已移除棋子: {id}");
    }

    public void RemoveAllPieces()
    {
        // 1. 從註冊表獲取所有當前的 PieceID 副本 (避免在迴圈中修改集合)
        var allIds = _viewRegistry.GetAllRegisteredIds().ToList();

        foreach (var id in allIds)
        {
            var view = _viewRegistry.GetView(id);
            if (view != null)
            {
                // 2. 銷毀場景上的 GameObject
                UnityEngine.Object.Destroy(view.gameObject);
            }

            // 3. 從註冊表中移除
            _viewRegistry.UnregisterView(id);
        }

        // 4. 清空 Spawner 內部的追蹤字典
        _pieceViews.Clear();
    }
    #endregion

    #region Private View管理方法
    private void CreatePieceView(ChessPiece piece, Team team)
    {
        Vector3 worldPos;
        if (piece.State == PieceState.InStandby)
        {
            // 若為待命區棋子，初始位置先給予 Vector3.zero
            // 隨後交由 StandbyZoneService 的邏輯去進行正確的排版 (RefreshLayout)
            worldPos = Vector3.zero;
        }
        else
        {
            Vector3 tileCenter = _tileGenerator.GetTileCenter(
              piece.CurrentPosition.X,
              piece.CurrentPosition.Y);

            float terrainHeight = _terrainSampler.GetTerrainHeightAt(
              piece.CurrentPosition.X,
              piece.CurrentPosition.Y);

            worldPos = new Vector3(
              tileCenter.x,
              terrainHeight,
              tileCenter.z);
        }

        var pieceView = _piecePrefabFactory.Create(piece.Type, piece, worldPos);

        pieceView.SetPosition(worldPos);
        float yRotation = team == Team.White ? 0f : 180f;
        pieceView.SetYRotation(yRotation);
        pieceView.SetMaterial(_settings.TeamMaterials[(int)team]);

        string targetLayer = (piece.State == PieceState.InStandby) ? "Standby" : "Pieces";
        pieceView.SetLayer(targetLayer);

        _pieceViews[piece.Id] = pieceView;

        _viewRegistry.RegisterView(piece.Id, pieceView);
    }

    private void DestroyPieceView(PieceID id)
    {
        var view = _viewRegistry.GetView(id);
        if (view != null)
        {
            _viewRegistry.UnregisterView(id);
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }

    private float GetPieceHeightOffset(ChessPieceType type)
    {
        switch (type)
        {
            case ChessPieceType.Pawn:
                return 0.05f;
            case ChessPieceType.King:
            case ChessPieceType.Queen:
                return 0.1f;
            default:
                return 0.05f;
        }
    }
    #endregion
}
