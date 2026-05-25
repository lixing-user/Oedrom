using UnityEngine;

public class CoordinateConverter : ICoordinateConverter
{
    private readonly BoardDimensions _dimensions;
    private readonly BoardSettings _settings;

    public CoordinateConverter(BoardDimensions dimensions, BoardSettings settings)
    {
        _dimensions = dimensions;
        _settings = settings;
    }


    #region 正向轉換 (World → Grid → Piece)
    public TilePosition WorldToGridIndex(Vector3 worldPos)
    {
        // 計算相對於棋盤左下角的位置
        float halfWidth = (_dimensions.BoardSizeX * _dimensions.TileSize) / 2f;
        float halfHeight = (_dimensions.BoardSizeY * _dimensions.TileSize) / 2f;

        // Convert world position to local position relative to the board center
        Vector3 localPos = worldPos - _settings.BoardCenter;

        // 將原點從中心移到左下角
        float offsetX = localPos.x + halfWidth;
        float offsetZ = localPos.z + halfHeight;

        // Calculate the grid position based on the tile size
        int gridX = Mathf.FloorToInt(offsetX / _dimensions.TileSize);
        int gridY = Mathf.FloorToInt(offsetZ / _dimensions.TileSize);

        //Debug.Log($"[WorldToGridIndex] World: {worldPos}, Local: {localPos}, Grid: ({gridX}, {gridY})");

        if (gridX < 0 || gridX >= _dimensions.BoardSizeX ||
            gridY < 0 || gridY >= _dimensions.BoardSizeY)
        {
            Debug.LogWarning($"[WorldToGridIndex] 座標超出範圍！原始: ({gridX}, {gridY})");
            gridX = Mathf.Clamp(gridX, 0, _dimensions.BoardSizeX - 1);
            gridY = Mathf.Clamp(gridY, 0, _dimensions.BoardSizeY - 1);
            Debug.LogWarning($"[WorldToGridIndex] Clamp 後: ({gridX}, {gridY})");
        }

        return new TilePosition(gridX, gridY);
    }

    public PiecePosition GridIndexToPiecePosition(TilePosition gridPos)
    {
        return PiecePosition.Create(gridPos.X, gridPos.Y, _dimensions);
    }

    public PiecePosition WorldToPiecePosition(Vector3 worldPos)
    {
        var tilePos = WorldToGridIndex(worldPos);
        return GridIndexToPiecePosition(tilePos);
    }
    #endregion


    #region 反向轉換 (Piece → Grid → World)
    /// 棋子邏輯座標 → 世界座標
    public Vector3 PieceToWorldPosition(PiecePosition piecePos)
    {
        // 1. 計算 XZ 平面座標（棋盤格子的中心）
        float x = (piecePos.X + 0.5f) * _dimensions.TileSize;
        float z = (piecePos.Y + 0.5f) * _dimensions.TileSize;

        // 2. 計算相對於棋盤左下角的偏移
        float halfWidth = (_dimensions.BoardSizeX * _dimensions.TileSize) / 2f;
        float halfHeight = (_dimensions.BoardSizeY * _dimensions.TileSize) / 2f;

        // 3. 轉換為世界座標
        Vector3 worldPos = new Vector3(
            x - halfWidth,
            0,  // 先設為 0，稍後加入地形高度
            z - halfHeight
        ) + _settings.BoardCenter;

        return worldPos;
    }

    public Vector3 TileToWorldPosition(TilePosition tilePos)
    {
        // 轉換為 PiecePosition 後再轉世界座標
        var piecePos = GridIndexToPiecePosition(tilePos);
        return PieceToWorldPosition(piecePos);
    }

    // piecePos 轉換為 GridIndex 的世界座標（不含地形高度）
    public Vector2 PieceToGridIndex(PiecePosition piecePos)
    {
        float x = (piecePos.X + 0.5f) * _dimensions.TileSize;
        float z = (piecePos.Y + 0.5f) * _dimensions.TileSize;
        float halfWidth = (_dimensions.BoardSizeX * _dimensions.TileSize) / 2f;
        float halfHeight = (_dimensions.BoardSizeY * _dimensions.TileSize) / 2f;
        Vector3 worldPos = new Vector3(
            x - halfWidth,
            0,
            z - halfHeight
        ) + _settings.BoardCenter;
        return worldPos;
    }
    #endregion
}


public interface ICoordinateConverter
{
    /// <summary>世界座標 → 網格索引 (TilePosition)</summary>
    TilePosition WorldToGridIndex(Vector3 worldPos);

    /// <summary>網格索引 → 棋盤邏輯座標 (PiecePosition)</summary>
    PiecePosition GridIndexToPiecePosition(TilePosition gridPos);

    /// <summary>世界座標 → 棋盤邏輯座標 (PiecePosition)</summary>
    PiecePosition WorldToPiecePosition(Vector3 worldPos);

    /// <summary>棋子邏輯座標 → 世界座標（含地形高度）</summary>
    Vector3 PieceToWorldPosition(PiecePosition piecePos);

    /// <summary>Tile 座標 → 世界座標</summary>
    Vector3 TileToWorldPosition(TilePosition tilePos);

    /// <summary>棋子邏輯座標 → 網格索引的世界座標（不含地形高度）</summary>
    Vector2 PieceToGridIndex(PiecePosition piecePos);
}