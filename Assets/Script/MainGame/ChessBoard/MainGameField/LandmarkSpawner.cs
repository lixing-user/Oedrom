using UnityEngine;
using Zenject;

public interface ILandmarkSpawner
{
    void SpawnLandmark(LandmarkType type, int x, int y);
}


public class LandmarkSpawner : ILandmarkSpawner
{
    [Inject] private readonly GameBoard<TerrainTile> _gameBoard;
    [Inject] private readonly ILandmarkViewRegistry _landmarkRegistry;
    [Inject] private readonly LandmarkView.Factory _factory;
    [Inject] private readonly BoardDimensions _dimensions;
    [Inject] private readonly IBoardTileGenerator _tileGenerator;

    public void SpawnLandmark(LandmarkType type, int x, int y)
    {
        try
        {
            var position = PiecePosition.Create(x, y, _dimensions);
            Vector3 worldPosition = _tileGenerator.GetTileCenter(x, y);

            // 1. 生成視覺物件 (View)
            var landmark = _factory.Create(type, position, worldPosition);

            _landmarkRegistry.Register(new Vector2Int(x, y), landmark);

            // 2. 更新領域資料 (Domain)
            var tile = _gameBoard.GetTileAt(position);
            if (tile != null)
            {
                tile.SetLandmark(type);
                Debug.Log($"[LandmarkSpawner] 已生成地標 {type} at ({x},{y})");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"生成地標失敗 [{type}] at ({x},{y}): {ex.Message}");
        }
    }
}
