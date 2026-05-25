using UnityEngine;
using Zenject;

public class TilePositionCalculator : ITilePositionCalculator
{
    [Inject(Id = "BoardTransform")]
    private readonly Transform _boardTransform;

    private readonly BoardDimensions _dimensions;
    private readonly ITileHeightCalculator _heightCalc;
    private readonly Vector3 _bounds;

    public TilePositionCalculator(
        BoardDimensions dimensions,
        ITileHeightCalculator heightCalc)
    {
        _dimensions = dimensions;
        _heightCalc = heightCalc;
    }

    public Vector3 CalculateTileLocalCenter(int x, int y)
    {
        Vector3 bounds = _dimensions.GridBounds;

        Vector3 gridPosition = new Vector3(
            (x + 0.5f) * _dimensions.TileSize,  
            0,
            (y + 0.5f) * _dimensions.TileSize
            ) ;

        Vector3 localOffset = gridPosition - bounds;

        Vector3 worldPosForHeight = _boardTransform.TransformPoint(localOffset);
        float height = _heightCalc.CalculateTileHeight(worldPosForHeight, localOffset);

        return new Vector3(localOffset.x, height, localOffset.z);
    }

    public Vector3[] CalculateTileVertices(int x, int y)
    {
        float tileSize = _dimensions.TileSize;

        float halfTile = tileSize / 2f;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-halfTile, 0, -halfTile);  // 左下
        vertices[1] = new Vector3(-halfTile, 0, halfTile);  // 左上
        vertices[2] = new Vector3(halfTile, 0, -halfTile);  // 右下
        vertices[3] = new Vector3(halfTile, 0, halfTile);  // 右上

        // 計算中心高度並應用到所有頂點
        Vector3 center = CalculateTileLocalCenter(x, y);
        for (int i = 0; i < 4; i++)
        {
            vertices[i].y = center.y;
        }

        return vertices;
    }

    public float CalculateTileHeight(Vector3 localPosition)
    {
        return _heightCalc.CalculateTileHeight(localPosition, localPosition);
    }
}

public interface ITilePositionCalculator
{
    /// <summary>計算 Tile 的本地中心座標（相對於棋盤中心）</summary>
    Vector3 CalculateTileLocalCenter(int x, int y);

    /// <summary>計算 Tile 的四個頂點本地座標</summary>
    Vector3[] CalculateTileVertices(int x, int y);

    /// <summary>計算 Tile 的高度</summary>
    float CalculateTileHeight(Vector3 localPosition);
}