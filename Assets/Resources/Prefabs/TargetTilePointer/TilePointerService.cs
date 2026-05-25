using UnityEngine;
using Zenject;

public interface ITilePointerService
{
    void ShowPointerAt(Vector2Int gridPos);
    void ClearPointer();
}

public class TilePointerService : ITilePointerService
{
    private readonly TilePointerView.Pool _pointerPool;
    private readonly ICoordinateConverter _coordinateConverter; // 用你現有的轉換器
    private readonly ITerrainHeightSampler _heightSampler;

    private TilePointerView _activePointer;

    public TilePointerService(
        TilePointerView.Pool pointerPool,
        ICoordinateConverter coordinateConverter,
        ITerrainHeightSampler heightSampler)
    {
        _pointerPool = pointerPool;
        _coordinateConverter = coordinateConverter;
        _heightSampler = heightSampler;
    }

    public void ShowPointerAt(Vector2Int gridPos)
    {
        ClearPointer(); // 確保同時間只有一個教學箭頭

        // 1. 將 Unity 原生的 Vector2Int 轉換為你的網格模型 TilePosition
        var tilePos = new TilePosition(gridPos.x, gridPos.y);

        // 2. 取得基礎世界座標 (主要依賴它的 X 與 Z 平面位置)
        Vector3 baseWorldPos = _coordinateConverter.TileToWorldPosition(tilePos);

        // 3. 透過 Sampler 取得該點真實的地形高度
        float terrainY = _heightSampler.GetTerrainHeightAt(baseWorldPos);

        // 4. 組合最終含有正確高度的世界座標
        Vector3 finalWorldPos = new Vector3(baseWorldPos.x, terrainY, baseWorldPos.z);

        _activePointer = _pointerPool.Spawn(finalWorldPos);
    }

    public void ClearPointer()
    {
        if (_activePointer != null)
        {
            _activePointer.Despawn(_pointerPool);
            _activePointer = null;
        }
    }
}
