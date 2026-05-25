using UnityEngine;

/// <summary>
/// 棋子移動記錄（用於計算補給消耗）
/// </summary>
public class PieceMovementRecord
{
    public PieceID PieceId { get; set; }
    public Vector2Int FromPosition { get; set; }
    public Vector2Int ToPosition { get; set; }
    public float MovedAt { get; set; }  // 移動時間（用於排序）
}
