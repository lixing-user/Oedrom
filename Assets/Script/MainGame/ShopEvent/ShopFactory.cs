using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 負責「複雜物件的生成邏輯（如隨機演算法、機率）」
public class ShopFactory : IShopFactory
{
    public List<ShopItemData> GenerateRandomShopItems()
    {
        var availableTypes = new List<ChessPieceType>
        {
            ChessPieceType.Pawn,
            ChessPieceType.Rook,
            ChessPieceType.Knight,
            ChessPieceType.Bishop
        };

        // 1. 打亂順序並取前 3 個
        var selectedTypes = availableTypes
            .OrderBy(_ => Guid.NewGuid()) // 替代 Random.value 的做法
            .Take(3)
            .ToList();

        var initialItems = new List<ShopItemData>();

        // 2. 將選中的種類轉換為商品資料 (設定價格與 ID)
        foreach (var type in selectedTypes)
        {
            int cost = DetermineCost(type); // 根據類型決定價格
            string uniqueId = Guid.NewGuid().ToString(); // 給予唯一 ID 以便追蹤與購買

            var item = new ShopItemData(uniqueId, type, cost);
            initialItems.Add(item);
        }

        // 3. (可選) 加入固定販售的補給品
        //initialItems.Add(new ShopItemData(Guid.NewGuid().ToString(), supplyAmount: 500, cost: 2000));

        // 4. 建立並回傳 Domain 模型
        return initialItems;
    }

    private int DetermineCost(ChessPieceType type)
    {
        switch (type)
        {
            case ChessPieceType.Pawn: return 1000;
            case ChessPieceType.Knight:
            case ChessPieceType.Bishop: return 3000;
            case ChessPieceType.Rook: return 5000;
            default: return 1000;
        }
    }
}

public interface IShopFactory
{
    List<ShopItemData> GenerateRandomShopItems();
}