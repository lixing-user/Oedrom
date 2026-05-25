using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

// 視覺效果子系統介面
public interface IBoardVisualizer
{
    void HighlightTiles(List<Vector2Int> moves);
    void RemoveHighlightTiles();
    void UpdateHoverTiles(Vector2Int hitPosition);
    void RemoveHoverTile();
    void SetPersistentHighlights(List<Vector2Int> moves);
    void ClearPersistentHighlights();
}

public class BoardVisualizer : IBoardVisualizer
{
    [Inject] private readonly BoardDimensions _board;
    [Inject] private readonly IBoardTileGenerator _tileGenerator;

    // 緩存 Tile 物件引用
    private Dictionary<Vector2Int, TileView> _tileCache = new Dictionary<Vector2Int, TileView>();

    private Vector2Int? previousHoverTile;
    private List<Vector2Int> _currentHighlights = new();
    private List<Vector2Int> _persistentHighlights = new();

    private bool _isInitialized = false;

    #region 初始化
    public void Initialize()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("[BoardVisualizer] 已經初始化，跳過");
            return;
        }

        Debug.Log("[BoardVisualizer] 開始建立 Tile 緩存...");
        BuildTileCache();

        _isInitialized = true;
    }

    private void EnsureInitialized()
    {
        if (_isInitialized) return;

        Debug.LogWarning("[BoardVisualizer] 延遲初始化 - 在第一次使用時建立緩存");
        Initialize();
    }
    #endregion

    #region 核心業務方法

    #region 一般遊玩高光 (暫時性)
    public void HighlightTiles(List<Vector2Int> moves)
    {
        EnsureInitialized();
        RemoveHighlightTiles();

        foreach (var move in moves)
        {
            SetTileHighlightLayer(move, true);
        }
        _currentHighlights = new List<Vector2Int>(moves);
    }

    public void RemoveHighlightTiles()
    {
        EnsureInitialized();

        foreach (var pos in _currentHighlights)
        {
            if (!_persistentHighlights.Contains(pos))
            {
                SetTileHighlightLayer(pos, false);
            }
        }
        _currentHighlights.Clear();
    }
    #endregion

    #region 教學系統高光 (常駐性)
    public void SetPersistentHighlights(List<Vector2Int> moves)
    {
        EnsureInitialized();
        ClearPersistentHighlights(); // 先清除舊的常駐

        foreach (var move in moves)
        {
            SetTileHighlightLayer(move, true);
        }
        _persistentHighlights = new List<Vector2Int>(moves);
        Debug.Log($"[BoardVisualizer] 設定了 {_persistentHighlights.Count} 個常駐高光");
    }

    public void ClearPersistentHighlights()
    {
        EnsureInitialized();
        foreach (var pos in _persistentHighlights)
        {
            // 核心邏輯：如果玩家剛好也選中這個格子(在暫時高光內)，就保留高光
            if (!_currentHighlights.Contains(pos))
            {
                SetTileHighlightLayer(pos, false);
            }
        }
        _persistentHighlights.Clear();
        Debug.Log("[BoardVisualizer] 已清除常駐高光");
    }
    #endregion

    #region 鼠標懸停高光
    public void UpdateHoverTiles(Vector2Int hitPosition)
    {
        EnsureInitialized();

        if (previousHoverTile.HasValue && previousHoverTile.Value == hitPosition)
        {
            return;  // 位置沒變，直接返回
        }

        // 1. 清除舊的 Hover 狀態
        if (previousHoverTile.HasValue)
        {
            if (_tileCache.TryGetValue(previousHoverTile.Value, out TileView prevTile))
            {
                prevTile.SetHoverState(false); // 關閉空心紅框
            }
        }

        // 設置新的 hover tile
        if (_tileCache.TryGetValue(hitPosition, out TileView newTile))
        {
            newTile.SetHoverState(true);  // 開啟空心紅框
            previousHoverTile = hitPosition;
        }
        else
        {
            previousHoverTile = null;
        }
    }

    public void RemoveHoverTile()
    {
        EnsureInitialized();

        if (previousHoverTile.HasValue)
        {
            if (_tileCache.TryGetValue(previousHoverTile.Value, out TileView prevTile))
            {
                prevTile.SetHoverState(false);
            }
            previousHoverTile = null;
        }
    }
    #endregion

    private void SetTileHighlightLayer(Vector2Int pos, bool isHighlight)
    {
        if (_tileCache.TryGetValue(pos, out TileView tileObj))
        {
            tileObj.gameObject.layer = LayerMask.NameToLayer(isHighlight ? "Highlight" : "Tiles");
        }
    }
    #endregion

    #region 緩存管理
    /// <summary>
    /// 建立 Tile 物件緩存（一次性操作）
    /// </summary>
    private void BuildTileCache()
    {
        _tileCache.Clear();

        int successCount = 0;
        int failCount = 0;

        for (int x = 0; x < _board.BoardSizeX; x++)
        {
            for (int y = 0; y < _board.BoardSizeY; y++)
            {
                // 直接從 TileGenerator 獲取（需要先添加這個方法）
                //GameObject tileObj = GetTileObjectDirect(x, y);
                TileView tileObj = GetTileObjectDirect(x, y);

                if (tileObj != null)
                {
                    _tileCache[new Vector2Int(x, y)] = tileObj;
                    successCount++;
                }
                else
                {
                    Debug.LogWarning($"[BoardVisualizer] 無法找到 Tile ({x}, {y})");
                    failCount++;
                }
            }
        }

        Debug.Log($"[BoardVisualizer] 緩存建立完成：成功 {successCount}，失敗 {failCount}");
    }

    /// <summary>
    /// 清除緩存
    /// </summary>
    public void ClearCache()
    {
        _tileCache.Clear();
    }
    #endregion

    #region 輔助方法
    private bool IsPositionValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _board.BoardSizeX && pos.y >= 0 && pos.y < _board.BoardSizeY;
    }

    private TileView GetTileObjectDirect(int x, int y)
    {
        // 方法 1：如果 TileGenerator 有提供直接訪問方法
        if (_tileGenerator is BoardTileGenerator generator)
        {
            GameObject obj = generator.GetTileObject(x, y);
            if (obj != null) return obj.GetComponent<TileView>();
        }

        // 方法 2：備用方案 - 使用 OverlapSphere（比 Raycast 更可靠）
        Vector3 tileCenter = _tileGenerator.GetTileCenter(x, y);
        Collider[] colliders = Physics.OverlapSphere(
            tileCenter,
            0.2f,
            LayerMask.GetMask("Tiles", "Hover", "Highlight")
        );

        return colliders.Length > 0 ? colliders[0].GetComponent<TileView>() : null;
    }
    #endregion

}
