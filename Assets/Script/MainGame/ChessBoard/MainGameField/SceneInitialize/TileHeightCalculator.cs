using UnityEngine;
using Zenject;

public class TileHeightCalculator : ITileHeightCalculator
{
    private readonly ITerrainDataService _terrainService;
    private readonly MarchingCubeSetting _settings;
    private readonly BoardDimensions _boardDimensions;
    private readonly BoardSettings _boardSettings;

    // Raycast 相關設定
    private const float RAY_START_HEIGHT = 100f;
    private const float RAY_MAX_DISTANCE = 200f;
    private const float RAY_DEBUG_DURATION = 2f;

    private readonly int _terrainLayerMask;
    //private float _heightOffset = 0.05f;

    public TileHeightCalculator(
      ITerrainDataService terrainService,
      MarchingCubeSetting settings,
      BoardDimensions boardDimensions,
      BoardSettings boardSettings)
    {
        _terrainService = terrainService;
        _settings = settings;
        _boardDimensions = boardDimensions;
        _boardSettings = boardSettings;

        _terrainLayerMask = LayerMask.GetMask("Terrain");
    }

    public float CalculateTileHeight(Vector3 worldPosition, Vector3 tileCenter)
    {
        float raycastHeight = GetHeightByRaycast(worldPosition);

        if (raycastHeight > 0)
        {
            //return raycastHeight + _heightOffset;
            return raycastHeight + _boardSettings.YOffset;
        }

        // 回退到數據查詢（僅用於地形尚未生成時）
        Debug.LogWarning($"[TileHeightCalculator] Raycast 失敗於 {worldPosition}，回退到數據查詢");
        return GetHeightByData(worldPosition);  
    }


    /// <summary>
    /// 使用 Raycast 查詢地形高度（主要方法）
    /// </summary>
    private float GetHeightByRaycast(Vector3 worldPosition)
    {
        // 從高處向下發射射線
        Vector3 rayStart = new Vector3(
            worldPosition.x,
            RAY_START_HEIGHT,
            worldPosition.z
        );

        Vector3 rayDirection = Vector3.down;

        // 除錯用射線可視化
#if UNITY_EDITOR
        Debug.DrawRay(
            rayStart,
            rayDirection * RAY_MAX_DISTANCE,
            Color.cyan,
            RAY_DEBUG_DURATION
        );
#endif

        // 執行 Raycast
        if (Physics.Raycast(
            rayStart,
            rayDirection,
            out RaycastHit hit,
            RAY_MAX_DISTANCE,
            _terrainLayerMask))
        {
            // 成功擊中地形
#if UNITY_EDITOR
            Debug.DrawLine(rayStart, hit.point, Color.green, RAY_DEBUG_DURATION);
#endif

            return hit.point.y;
        }

        // 未擊中任何物體
#if UNITY_EDITOR
        Debug.DrawRay(rayStart, rayDirection * RAY_MAX_DISTANCE, Color.red, RAY_DEBUG_DURATION);
#endif

        return 0f; // 返回 0 表示失敗
    }


    /// <summary>
    /// 使用原始數據查詢（回退方案）
    /// </summary>
    private float GetHeightByData(Vector3 worldPosition)
    {
        Vector3 boardCenter = _boardSettings.BoardCenter;
        Vector3 bounds = _boardDimensions.GridBounds;

        if (_settings.use3DNoise)
        {
            Vector3Int heightIndex = _terrainService.WorldToHeightIndex(worldPosition);
            float terrainHeight = _terrainService.GetHeightAtIndex(
                heightIndex.x, heightIndex.y, heightIndex.z);
            //return terrainHeight * _settings.height * _settings.resolution + _heightOffset;
            return terrainHeight * _settings.height * _settings.resolution + _boardSettings.YOffset;
        }
        else
        {
            float localX = (worldPosition.x - (boardCenter.x - bounds.x)) / _settings.resolution;
            float localZ = (worldPosition.z - (boardCenter.z - bounds.z)) / _settings.resolution;
            //return _terrainService.GetSurfaceHeight(localX, localZ) + _heightOffset;
            return _terrainService.GetSurfaceHeight(localX, localZ) + _boardSettings.YOffset;
        }
    }
}

public interface ITileHeightCalculator
{
    float CalculateTileHeight(Vector3 worldPosition, Vector3 tileCenter);
}