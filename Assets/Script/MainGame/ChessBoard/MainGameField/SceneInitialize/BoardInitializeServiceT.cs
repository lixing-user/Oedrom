using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class BoardInitializeServiceT : IBoardInitializationServiceT
{
    [Inject] private readonly GameBoard<TerrainTile> _gameBoard;
    [Inject] private readonly BoardDimensions _dimensions; // Domain
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly IBoardQueryService _boardQuery;

    [Inject] private readonly ITerrainTypeClassifier _terrainClassifier;
    [Inject] private readonly IBoardTileGenerator _tileGenerator; // Infrastructure
    [Inject] private readonly ILandmarkSpawner _landmarkSpawner; 
    [Inject] private readonly IPieceSpawner _pieceSpawner;


    public IEnumerator InitializeBoardAsync(MarchingCubes terrain)
    {
        #region 前置檢查
        while (terrain?.heights == null)
            yield return null;
        Debug.Log("[BoardInitialize] 地形高度資料已就緒");


        var meshFilter = terrain.GetComponent<MeshFilter>();
        while (meshFilter == null || meshFilter.mesh == null)
            yield return null;
        Debug.Log($"[BoardInitialize] Mesh 已生成（頂點數：{meshFilter.mesh.vertexCount}）");


        var meshCollider = terrain.GetComponent<MeshCollider>();
        while (meshCollider == null || meshCollider.sharedMesh == null)
            yield return null;
        Debug.Log("[BoardInitialize] MeshCollider 已設定");
        #endregion

        // 1. 生成棋盤格子
        _tileGenerator.GenerateAllTiles(
            _dimensions.GridSpacing,
            _dimensions.BoardSizeX,
            _dimensions.BoardSizeY
        );//注意，棋盤的高度沒有對齊，請檢查calculator或heightadapter

        // 2. 生成地形類型
        yield return SpawnTerrainTile();
        
        // 3. 生成地標
        yield return SpawnLandmarks();

        // 4. 生成棋子
        yield return SpawnAllPieces();

        MarkEnemyKing();

        Debug.Log("棋盤初始化完成");
    }

    private IEnumerator SpawnAllPieces()
    {
        int boardSize = _dimensions.BoardSizeX;

        #region 正式棋盤（FEN解析）
        foreach (var pieceData in TutorialSetup.TutorialPieces)
        {
            SpawnPiece(pieceData.Type, pieceData.X, pieceData.Y, pieceData.Team);
        }
        #endregion
        yield return null;
    }
    private void SpawnPiece(ChessPieceType type, int x, int y, Team team)
    {
        try
        {
            var position = PiecePosition.Create(x, y, _dimensions);
            _pieceSpawner.SpawnSinglePiece(type, position, team);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"生成棋子失敗 [{type}] at ({x},{y}): {ex.Message}");
            Debug.LogError(ex.StackTrace);
        }
    }

    private void MarkEnemyKing()
    {
        if (_boardQuery == null) return;

        var blackPieces = _boardQuery.GetPiecesForTeam(Team.Black);
        var enemyKing = blackPieces.FirstOrDefault(p => p.Type == ChessPieceType.King);

        if (enemyKing != null)
        {
            // 發射訊號標記國王
            _signalBus.Fire(new MarkTargetPieceSignal
            {
                TargetPieceId = enemyKing.Id,
                IsPrimary = true
            });
            Debug.Log($"[BoardInitializeT] 成功鎖定並標記黑方國王: {enemyKing.Id}");
        }
        else
        {
            Debug.LogWarning("[BoardInitializeT] 找不到黑方國王可以標記！");
        }
    }

    private IEnumerator SpawnTerrainTile()
    {
        var terrainTiles = new List<TerrainTile>();

        for (int x = 0; x < _dimensions.BoardSizeX; x++)
        {
            for (int y = 0; y < _dimensions.BoardSizeY; y++)
            {
                // 從已生成的 Tile 獲取高度
                Vector3 tileCenter = _tileGenerator.GetTileCenter(x, y);
                float height = tileCenter.y; // 例如：6.3f

                // 判定地形類型
                TerrainType terrain = _terrainClassifier.ClassifyByHeight(height);
                // 輸出範例：height=6.3f → TerrainType.Forest

                // 創建 Domain 模型
                var position = PiecePosition.Create(x, y, _dimensions);
                var tile = new TerrainTile(position, terrain);

                // 添加到聚合根
                _gameBoard.AddTile(tile);
                terrainTiles.Add(tile);

                Debug.Log($"Tile({x},{y}): Y={height:F2} → {terrain}");
            }
        }

        yield return null;
    }

    private IEnumerator SpawnLandmarks()
    {
        Debug.Log("[BoardInitialize] 開始生成地標");

        // 教學關卡：在特定座標生成 Town
        //SpawnLandmark(LandmarkType.Town, 2, 6);
        //SpawnLandmark(LandmarkType.Town, 7, 3);
        //SpawnLandmark(LandmarkType.Town, 5, 4);

        foreach (var landmark in TutorialSetup.TutorialLandmarks)
        {
            SpawnLandmark(landmark.Type, landmark.X, landmark.Y);
        }

        Debug.Log("[BoardInitialize] 地標生成完成");
        yield return null;
    }
    private void SpawnLandmark(LandmarkType type, int x, int y)
    {
        try
        {
            _landmarkSpawner.SpawnLandmark(type, x, y);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"生成地標失敗 [{type}] at ({x},{y}): {ex.Message}");
        }
    }
}

public interface IBoardInitializationServiceT
{
    IEnumerator InitializeBoardAsync(MarchingCubes terrain);
}

public class BoardInitializedSignal{}