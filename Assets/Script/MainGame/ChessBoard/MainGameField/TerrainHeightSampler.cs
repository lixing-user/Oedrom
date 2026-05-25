using UnityEngine;
using Zenject;

//  用映射方式讓物件貼合地形高度
public class TerrainHeightSampler : ITerrainHeightSampler
{
    [Inject] private readonly IBoardTileGenerator _tileGenerator;
    [Inject] private readonly BoardSettings _settings;

    private const float RAY_START_HEIGHT = 100f;  // 射線起點高度
    private const float RAY_MAX_DISTANCE = 200f;  // 射線最大距離
    private const float RAY_DISPLAY_DURATION = 2f;

    private readonly int _terrainLayerMask = LayerMask.GetMask("Terrain");


    public float GetTerrainHeightAt(int x, int y)
    {
        // 1. 獲取該格子的水平中心座標（XZ 平面）
        Vector3 tileCenter = _tileGenerator.GetTileCenter(x, y);

        // 2. 忽略其 Y，從上方投射射線
        Vector3 rayStart = new Vector3(
            tileCenter.x,
            RAY_START_HEIGHT,  // 從高處往下射
            tileCenter.z
        );


        Vector3 rayDirection = Vector3.down;

        // 顯示射線（永遠顯示，用於除錯）
        Debug.DrawRay(
            rayStart,
            rayDirection * RAY_MAX_DISTANCE,
            Color.yellow,      // 預設黃色（表示正在射）
            RAY_DISPLAY_DURATION
        );

        Physics.SyncTransforms();

        // 3. 向下投射射線
        if (Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            RAY_MAX_DISTANCE,
            _terrainLayerMask))
        {
            return hit.point.y;  // 返回碰撞點的世界 Y
        }

        // 4. 若未碰撞，返回預設平面高度
        Debug.LogWarning($"[TerrainHeightSampler] 未找到地形高度 at ({x}, {y})");
        return _settings.BoardCenter.y;  // 或其他預設值
    }

    public float GetTerrainHeightAt(Vector3 worldXZ)
    {
        Vector3 rayStart = new Vector3(worldXZ.x, RAY_START_HEIGHT, worldXZ.z);

        Debug.DrawRay(rayStart, Vector3.down * RAY_MAX_DISTANCE, Color.cyan, 0.5f);

        if (Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            RAY_MAX_DISTANCE,
            _terrainLayerMask))
        {
            return hit.point.y + 0.05f;  // 稍微抬高，避免 Z-fighting
        }

        return _settings.BoardCenter.y;
    }
}

public interface ITerrainHeightSampler
{
    /// <summary>
    /// 根據棋盤邏輯座標獲取地形高度
    /// </summary>
    /// <param name="x">棋盤 X 座標（0-7）</param>
    /// <param name="y">棋盤 Y 座標（0-7）</param>
    /// <returns>該位置的地形世界 Y 高度（若無地形返回 0）</returns>
    float GetTerrainHeightAt(int x, int y);

    /// <summary>
    /// 根據世界座標 XZ 獲取地形高度（更精確）
    /// </summary>
    float GetTerrainHeightAt(Vector3 worldXZ);
}