using System.Collections.Generic;
using UnityEngine;

public class TileOccupancyManager
{
    private static readonly Vector3[] SubPositions = new[]
   {
        Vector3.zero,                        // 中心
        new Vector3(-0.4f, 0, 0),            // 左
        new Vector3(0.4f, 0, 0),             // 右
        new Vector3(0, 0, -0.4f),            // 前
        new Vector3(0, 0, 0.4f),             // 後
        new Vector3(-0.4f, 0, -0.4f),        // 左前
        new Vector3(0.4f, 0, -0.4f),         // 右前
        new Vector3(-0.4f, 0, 0.4f),         // 左後
        new Vector3(0.4f, 0, 0.4f)           // 右後
    };

    private Dictionary<Vector2Int, List<PieceID>> _tileOccupancy = new();

    public Vector3 GetAvailablePosition(Vector2Int tilePos, Vector3 tileCenter, PieceID pieceId)
    {
        if (!_tileOccupancy.ContainsKey(tilePos))
            _tileOccupancy[tilePos] = new List<PieceID>();

        var occupants = _tileOccupancy[tilePos];
        int index = occupants.Count;  // 第幾個進入

        occupants.Add(pieceId);

        // 超過 9 個就疊加（罕見情況）
        if (index >= SubPositions.Length)
            index = SubPositions.Length - 1;

        return tileCenter + SubPositions[index];
    }

    public void RemovePiece(Vector2Int tilePos, PieceID pieceId)
    {
        if (_tileOccupancy.TryGetValue(tilePos, out var occupants))
        {
            occupants.Remove(pieceId);
        }
    }
}
