using System;
using UnityEngine;


// 基本地形格子
public class Tile 
{
    public PiecePosition Position { get; }

    public Tile(PiecePosition position)
    {
        Position = position;
    }

    public virtual bool CanEnter(ChessPieceType pieceType) => true; // 預設所有棋子都能進入
}


// 主遊戲場景地形格子
public class TerrainTile : Tile
{
    public TerrainType Terrain { get; private set; }
    public LandmarkType Landmark { get; private set; }

    private bool _isEventTriggered = false;
    public bool IsEventTriggered => _isEventTriggered;


    public TerrainTile(PiecePosition position, TerrainType terrain) : base(position)
    {
        Terrain = terrain;
        Landmark = LandmarkType.None;
    }


    #region 業務方法
    // TerrainTile特有方法
    public void SetLandmark(LandmarkType landmark)
    {
        if (Terrain == TerrainType.Water)
            throw new InvalidOperationException("水域無法設置地標");
        Landmark = landmark;
    }

    public void MarkEventTriggered()
    {
        if (Landmark == LandmarkType.None)
            throw new InvalidOperationException("沒有地標的格子無法觸發事件");

        _isEventTriggered = true;
    }

    // 重置事件狀態（用於新回合或特殊情況）
    public void ResetEventTrigger()
    {
        _isEventTriggered = false;
    }
    #endregion
}
