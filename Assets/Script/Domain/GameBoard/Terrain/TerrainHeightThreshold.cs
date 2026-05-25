using System;
using UnityEngine;

/// <summary>
/// Domain Value Object：地形高度閾值配置
/// </summary>
[Serializable]
public class TerrainHeightThreshold
{
    [Header("地形高度閾值設定")]

    [Tooltip("水域最大高度")]
    public float waterMaxHeight = 2.0f;

    [Tooltip("平原最大高度")]
    public float plainMaxHeight = 5.0f;

    [Tooltip("森林最大高度")]
    public float forestMaxHeight = 8.0f;

    [Tooltip("山地高度（超過此值）")]
    public float mountainMinHeight = 8.0f;


    public TerrainType DetermineTerrainType(float height)
    {
        if (height <= waterMaxHeight)
            return TerrainType.Water;
        else if (height <= plainMaxHeight)
            return TerrainType.Plain;
        else if (height <= forestMaxHeight)
            return TerrainType.Forest;
        else
            return TerrainType.Mountain;
    }
}
