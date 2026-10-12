using UnityEngine;

public class FlatTileHeightCalculator : ITileHeightCalculator
{
    private readonly float _fixedHeight;

    public FlatTileHeightCalculator(BoardSettings settings)
    {
        _fixedHeight = settings.BoardCenter.y + settings.YOffset;
    }

    public float CalculateTileHeight(Vector3 worldPosition, Vector3 tileCenter)
    {
        // 平面棋盤：所有 Tile 都在同一高度
        return _fixedHeight;
    }
}
