using UnityEngine;
using Zenject;

public class CoordinateConverter : ICoordinateConverter
{
    #region Backup
    //private readonly BoardDimensions _dimensions;
    //private readonly BoardSettings _settings;

    //public CoordinateConverter(BoardDimensions dimensions, BoardSettings settings)
    //{
    //    _dimensions = dimensions;
    //    _settings = settings;
    //}


    //#region 正向轉換 (World → Grid → Piece)
    //public TilePosition WorldToGridIndex(Vector3 worldPos)
    //{
    //    // 計算相對於棋盤左下角的位置
    //    float halfWidth = (_dimensions.BoardSizeX * _dimensions.TileSize) / 2f;
    //    float halfHeight = (_dimensions.BoardSizeY * _dimensions.TileSize) / 2f;

    //    // Convert world position to local position relative to the board center
    //    Vector3 localPos = worldPos - _settings.BoardCenter;

    //    // 將原點從中心移到左下角
    //    float offsetX = localPos.x + halfWidth;
    //    float offsetZ = localPos.z + halfHeight;

    //    // Calculate the grid position based on the tile size
    //    int gridX = Mathf.FloorToInt(offsetX / _dimensions.TileSize);
    //    int gridY = Mathf.FloorToInt(offsetZ / _dimensions.TileSize);

    //    //Debug.Log($"[WorldToGridIndex] World: {worldPos}, Local: {localPos}, Grid: ({gridX}, {gridY})");

    //    if (gridX < 0 || gridX >= _dimensions.BoardSizeX ||
    //        gridY < 0 || gridY >= _dimensions.BoardSizeY)
    //    {
    //        Debug.LogWarning($"[WorldToGridIndex] 座標超出範圍！原始: ({gridX}, {gridY})");
    //        gridX = Mathf.Clamp(gridX, 0, _dimensions.BoardSizeX - 1);
    //        gridY = Mathf.Clamp(gridY, 0, _dimensions.BoardSizeY - 1);
    //        Debug.LogWarning($"[WorldToGridIndex] Clamp 後: ({gridX}, {gridY})");
    //    }

    //    return new TilePosition(gridX, gridY);
    //}

    //public PiecePosition GridIndexToPiecePosition(TilePosition gridPos)
    //{
    //    return PiecePosition.Create(gridPos.X, gridPos.Y, _dimensions);
    //}

    //public PiecePosition WorldToPiecePosition(Vector3 worldPos)
    //{
    //    var tilePos = WorldToGridIndex(worldPos);
    //    return GridIndexToPiecePosition(tilePos);
    //}
    //#endregion


    //#region 反向轉換 (Piece → Grid → World)
    ///// 棋子邏輯座標 → 世界座標
    //public Vector3 PieceToWorldPosition(PiecePosition piecePos)
    //{
    //    // 1. 計算 XZ 平面座標（棋盤格子的中心）
    //    float x = (piecePos.X + 0.5f) * _dimensions.TileSize;
    //    float z = (piecePos.Y + 0.5f) * _dimensions.TileSize;

    //    // 2. 計算相對於棋盤左下角的偏移
    //    float halfWidth = (_dimensions.BoardSizeX * _dimensions.TileSize) / 2f;
    //    float halfHeight = (_dimensions.BoardSizeY * _dimensions.TileSize) / 2f;

    //    // 3. 轉換為世界座標
    //    Vector3 worldPos = new Vector3(
    //        x - halfWidth,
    //        0,  // 先設為 0，稍後加入地形高度
    //        z - halfHeight
    //    ) + _settings.BoardCenter;

    //    return worldPos;
    //}

    //public Vector3 TileToWorldPosition(TilePosition tilePos)
    //{
    //    // 轉換為 PiecePosition 後再轉世界座標
    //    var piecePos = GridIndexToPiecePosition(tilePos);
    //    return PieceToWorldPosition(piecePos);
    //}

    //// piecePos 轉換為 GridIndex 的世界座標（不含地形高度）
    //public Vector2 PieceToGridIndex(PiecePosition piecePos)
    //{
    //    float x = (piecePos.X + 0.5f) * _dimensions.TileSize;
    //    float z = (piecePos.Y + 0.5f) * _dimensions.TileSize;
    //    float halfWidth = (_dimensions.BoardSizeX * _dimensions.TileSize) / 2f;
    //    float halfHeight = (_dimensions.BoardSizeY * _dimensions.TileSize) / 2f;
    //    Vector3 worldPos = new Vector3(
    //        x - halfWidth,
    //        0,
    //        z - halfHeight
    //    ) + _settings.BoardCenter;
    //    return worldPos;
    //}
    //#endregion

    #endregion

    private readonly BoardDimensions _dimensions;
    private readonly Transform _boardTransform;

    public CoordinateConverter(
        BoardDimensions dimensions,
        [Inject(Id = "BoardTransform")] Transform boardTransform)
    {
        _dimensions = dimensions;
        _boardTransform = boardTransform;
    }

    #region 正向轉換 (World → Grid → Piece)
    public TilePosition WorldToGridIndex(Vector3 worldPos)
    {
        // 1. 將世界座標轉為相對於棋盤實體的本地座標 (解決原點與旋轉誤差)
        Vector3 localPos = _boardTransform.InverseTransformPoint(worldPos);

        // 2. 將原點從棋盤中心平移至左下角，使用統一的 GridBounds
        float offsetX = localPos.x + _dimensions.GridBounds.x;
        float offsetZ = localPos.z + _dimensions.GridBounds.z;

        // 3. 計算網格索引
        int gridX = Mathf.FloorToInt(offsetX / _dimensions.GridSpacing);
        int gridY = Mathf.FloorToInt(offsetZ / _dimensions.GridSpacing);

        if (gridX < 0 || gridX >= _dimensions.BoardSizeX ||
            gridY < 0 || gridY >= _dimensions.BoardSizeY)
        {
            Debug.LogWarning($"[WorldToGridIndex] 座標超出範圍！原始: ({gridX}, {gridY})");
            gridX = Mathf.Clamp(gridX, 0, _dimensions.BoardSizeX - 1);
            gridY = Mathf.Clamp(gridY, 0, _dimensions.BoardSizeY - 1);
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
    public Vector3 PieceToWorldPosition(PiecePosition piecePos)
    {
        // 1. 計算網格左下角的基準座標
        float x = (piecePos.X + 0.5f) * _dimensions.GridSpacing;
        float z = (piecePos.Y + 0.5f) * _dimensions.GridSpacing;

        // 2. 減去 GridBounds 偏移，回到相對於 Board 中心的本地座標
        Vector3 localPos = new Vector3(
            x - _dimensions.GridBounds.x,
            0,
            z - _dimensions.GridBounds.z
        );

        // 3. 轉回世界座標 (與 Tile 生成使用同一套 Transform 矩陣)
        return _boardTransform.TransformPoint(localPos);
    }

    public Vector3 TileToWorldPosition(TilePosition tilePos)
    {
        var piecePos = GridIndexToPiecePosition(tilePos);
        return PieceToWorldPosition(piecePos);
    }

    public Vector2 PieceToGridIndex(PiecePosition piecePos)
    {
        // 原本重複手寫了計算，現在統一經過 Transform 計算以確保無誤差
        Vector3 worldPos = PieceToWorldPosition(piecePos);
        return new Vector2(worldPos.x, worldPos.z);
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