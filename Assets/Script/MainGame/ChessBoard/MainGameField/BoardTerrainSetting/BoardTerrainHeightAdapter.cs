using UnityEngine;
using Zenject;

public interface ITerrainDataService
{
    Vector3 gridBounds { get; }
    Vector3 gridCenter { get; }
    bool IsInitialized { get; }

    Vector3Int WorldToHeightIndex(Vector3 worldPos);
    float GetSurfaceHeight(float x, float z);
    float GetHeightAtIndex(int x, int y, int z);
}

public class BoardTerrainHeightAdapter : ITerrainDataService, IInitializable
{
    [Inject] private BoardSettings _boardSettings;
    [Inject] private MarchingCubeSetting _marchingCubeSetting;

    [Inject] private readonly BoardDimensions _dimensions;


    public Vector3 gridBounds { get; set; }
    public Vector3 gridCenter { get; set; }

    private float ManualHeight = 6.0f;

    public bool IsInitialized { get; set; }
    public float[,,] Heights; // 公開 heights 陣列


    public void Initialize()
    {
        Heights = _marchingCubeSetting.heights;
        if (Heights != null)IsInitialized = true;

        gridCenter = _boardSettings.BoardCenter;
        gridBounds = _dimensions.GridBounds;

        float marchingWidth = _marchingCubeSetting.width * _marchingCubeSetting.resolution;
        float domainWidth = _dimensions.BoardSizeX * _dimensions.TileSize;

        if (!Mathf.Approximately(marchingWidth, domainWidth))
        {
            Debug.LogError($"座標系統不一致！Marching: {marchingWidth}, Domain: {domainWidth}");
        }
    }

    public Vector3Int WorldToHeightIndex(Vector3 worldPos)
    {
        // 將世界座標轉換為 heights 索引
        Vector3 localPos = worldPos - (gridCenter - gridBounds);
        int x = Mathf.Clamp(Mathf.FloorToInt(localPos.x / _marchingCubeSetting.resolution), 0, _marchingCubeSetting.width);
        int y = Mathf.Clamp(Mathf.FloorToInt(localPos.y / _marchingCubeSetting.resolution), 0, _marchingCubeSetting.height);
        int z = Mathf.Clamp(Mathf.FloorToInt(localPos.z / _marchingCubeSetting.resolution), 0, _marchingCubeSetting.width);
        return new Vector3Int(x, y, z);
    }

    public float GetSurfaceHeight(float x, float z)
    {
        if (!IsInitialized) return 0f;
        return _marchingCubeSetting.height * Mathf.PerlinNoise(
            x * _marchingCubeSetting.noiseScale,
            z * _marchingCubeSetting.noiseScale) * _marchingCubeSetting.resolution - ManualHeight;
    }

    public float GetHeightAtIndex(int x, int y, int z)
    {
        if (x >= 0 && x < _marchingCubeSetting.width + 1 && y >= 0 && y < _marchingCubeSetting.height + 1 && z >= 0 && z < _marchingCubeSetting.width + 1)
            return _marchingCubeSetting.heights[x, y, z];
        return 0f; // 超出範圍返回默認值
    }
}

