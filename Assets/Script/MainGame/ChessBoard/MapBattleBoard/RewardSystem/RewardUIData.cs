using UnityEngine;

// Application / DTO
/// <summary>
/// 卡牌獎勵
/// </summary>
public struct RewardUIData
{
    public PieceID Id;         // 追蹤用
    public ChessPieceType Type;
    public string DisplayName; // 顯示名稱
    public Sprite Icon;        // 卡面圖片
    public bool IsBroken;      // 是否顯示破損特效
    public int SupplyCost;     // 費用
}

/// <summary>
/// 資源獎勵
/// </summary>
public struct ResourceRewardData
{
    public float MoraleAmount;
    public float SupplyAmount;
    public float CoinAmount;
}