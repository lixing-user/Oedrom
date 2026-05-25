using UnityEngine;

/// <summary>
/// Domain Service：地形類型判定服務
/// </summary>
public interface ITerrainTypeClassifier
{
    TerrainType ClassifyByHeight(float height);
}


public class TerrainTypeClassifier : ITerrainTypeClassifier
{
    private readonly TerrainHeightThreshold _thresholds;

    public TerrainTypeClassifier(TerrainHeightThreshold thresholds)
    {
        _thresholds = thresholds;
    }

    public TerrainType ClassifyByHeight(float height)
    {
        return _thresholds.DetermineTerrainType(height);
    }
}