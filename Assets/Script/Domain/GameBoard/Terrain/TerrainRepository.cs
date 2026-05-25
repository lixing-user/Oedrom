using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainRepository : ITerrainRepository
{
    private readonly GameBoard<TerrainTile> _board;
    private readonly Dictionary<PiecePosition, TerrainTile> _tiles = new();

    public TerrainRepository(GameBoard<TerrainTile> board)
    {
        _board = board;
    }


    #region  查詢方法
    public TerrainTile GetTileAt(PiecePosition position)
    {
        return _board.GetTileAt(position);
    }

    public TerrainType GetTerrainAt(PiecePosition position)
    {
        var tile = GetTileAt(position);
        return tile?.Terrain ?? TerrainType.Plain; // 預設值
    }

    public LandmarkType GetLandmarkAt(PiecePosition position)
    {
        var tile = GetTileAt(position);
        return tile?.Landmark ?? LandmarkType.None;
    }


    public IEnumerable<TerrainTile> GetAllTiles()
    {
        return _tiles.Values;
    }

    public IEnumerable<TerrainTile> GetTilesWithLandmarks()
    {
        return _tiles.Values.Where(t => t.Landmark != LandmarkType.None);
    }
    #endregion


    #region 修改方法

    public void MarkEventTriggered(PiecePosition position)
    {
        var tile = GetTileAt(position);
        if (tile == null)
        {
            Debug.LogWarning($"嘗試標記不存在的格子: {position}");
            return;
        }

        try
        {
            tile.MarkEventTriggered();
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogWarning($"無法標記事件觸發: {ex.Message}");
        }
    }

    public void ResetEventTrigger(PiecePosition position)
    {
        var tile = GetTileAt(position);
        tile?.ResetEventTrigger();
    }

    #endregion
}

public interface ITerrainRepository
{
    TerrainTile GetTileAt(PiecePosition position);
    TerrainType GetTerrainAt(PiecePosition position);
    LandmarkType GetLandmarkAt(PiecePosition position);
    
    void MarkEventTriggered(PiecePosition position);

    IEnumerable<TerrainTile> GetAllTiles();
    IEnumerable<TerrainTile> GetTilesWithLandmarks();
}