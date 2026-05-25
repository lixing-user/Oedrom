using System;
using UnityEngine;

[Serializable]
public class StandbyLayoutConfig
{
    public LayoutMode LayoutMode = LayoutMode.Vertical;
    public float Spacing = 0.8f;
    public float BaseHeight = 0.5f;
    public int Columns = 4; // 供 Grid 模式使用
}

public class StandbyLayoutCalculator
{
    /// <summary>
    /// 根據傳入的配置與索引，計算出精準的 3D 座標
    /// </summary>
    public Vector3 CalculateLocalOffset(int index, StandbyLayoutConfig config)
    {
        Vector3 offset = config.LayoutMode switch
        {
            LayoutMode.Vertical => new Vector3(0, 0, index * config.Spacing),       // 前後排列
            LayoutMode.Horizontal => new Vector3(index * config.Spacing, 0, 0),     // 左右排列
            LayoutMode.Grid => CalculateGridOffset(index, config),          // 網格排列
            _ => new Vector3(0, 0, index * config.Spacing)
        };

        // 將算出的偏移量加上起始座標與基礎高度
        return new Vector3(offset.x, config.BaseHeight, offset.z);
    }

    private Vector3 CalculateGridOffset(int index, StandbyLayoutConfig config)
    {
        // 使用傳入的列數
        int row = index / config.Columns;
        int col = index % config.Columns;

        return new Vector3(col * config.Spacing, 0, row * config.Spacing);
    }

    public Vector3 CalculateWorldPosition(Vector3 anchorPosition, int index, StandbyLayoutConfig config)
    {
        return anchorPosition + CalculateLocalOffset(index, config);
    }
}

