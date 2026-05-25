using System.Collections.Generic;
using UnityEngine;

// Domain/Shop/ShopStateDTO.cs
public class ShopStateDTO
{
    public string ShopId { get; set; }
    public List<ShopItemData> AvailableItems { get; set; } = new List<ShopItemData>();
    public bool IsLooted { get; set; } // 標記是否已經被搶劫過（被搶過可能商店會關門或物品清空）

    public float PriceMultiplier { get; set; } = 1.0f;  // 可以記錄這間商店專屬的設定，例如黑市價格比較貴
}
