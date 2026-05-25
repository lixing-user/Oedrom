using ChessPieceCardProduction;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Domain/Shop.cs
public class Shop
{
    private List<ShopItemData> _availableItems = new List<ShopItemData>();

    public Shop(List<ShopItemData> items)
    {
        _availableItems = items;
    }

    // 回傳所有可購買的商品清單
    public IReadOnlyList<ShopItemData> GetAllItems()
    {
        return _availableItems.AsReadOnly();
    }

    public ShopItemData GetItem(string itemId)
    {
        return _availableItems.FirstOrDefault(i => i.Id == itemId);
    }

    public void RemoveItem(string itemId)
    {
        var item = GetItem(itemId);
        if (item != null)
        {
            _availableItems.Remove(item);
            // 可以在這裡觸發 Domain Event：_domainEvents.Add(new ItemRemovedEvent(itemId));
        }
    }

    public void Restock(List<ShopItemData> newItems)    // 用於初始化或重新進貨
    {
        _availableItems.Clear();
        _availableItems.AddRange(newItems);
    }
}
