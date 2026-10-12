using UnityEngine;
using Zenject;

public class TilePositionCalculator : ITilePositionCalculator
{
    [Inject(Id = "BoardTransform")]
    private readonly Transform _boardTransform;

    private readonly BoardDimensions _dimensions;
    private readonly ITileHeightCalculator _heightCalc;
    private readonly BoardSettings _settings;

    public TilePositionCalculator(
        BoardDimensions dimensions,
        ITileHeightCalculator heightCalc,
        BoardSettings settings)
    {
        _dimensions = dimensions;
        _heightCalc = heightCalc;
        _settings = settings;
    }

    public Vector3 CalculateTileLocalCenter(int x, int y)
    {
        Vector3 bounds = _dimensions.GridBounds;

        Vector3 gridPosition = new Vector3(
            (x + 0.5f) * _dimensions.GridSpacing,  
            0,
            (y + 0.5f) * _dimensions.GridSpacing
            ) ;

        Vector3 localOffset = gridPosition - bounds;

        Vector3 worldPosForHeight = _boardTransform.TransformPoint(localOffset);
        float worldHeight = _heightCalc.CalculateTileHeight(worldPosForHeight, localOffset);

        Vector3 worldPosWithHeight = new Vector3(worldPosForHeight.x, worldHeight, worldPosForHeight.z);
        return _boardTransform.InverseTransformPoint(worldPosWithHeight);
    }

    public Vector3[] CalculateTileVertices(int x, int y)
    {
        float visualSize = _settings.TileVisualSize;

        //float gridSpacing = _dimensions.GridSpacing;

        //float halfGrid = gridSpacing / 2f;
        float halfTile = visualSize / 2f;

        //Vector3[] vertices = new Vector3[4];
        //vertices[0] = new Vector3(-halfGrid, 0, -halfGrid);  // 左下
        //vertices[1] = new Vector3(-halfGrid, 0, halfGrid);  // 左上
        //vertices[2] = new Vector3(halfGrid, 0, -halfGrid);  // 右下
        //vertices[3] = new Vector3(halfGrid, 0, halfGrid);  // 右上

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-halfTile, 0, -halfTile);  // 左下
        vertices[1] = new Vector3(-halfTile, 0, halfTile);   // 左上
        vertices[2] = new Vector3(halfTile, 0, -halfTile);   // 右下
        vertices[3] = new Vector3(halfTile, 0, halfTile);    // 右上

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