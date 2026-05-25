using UnityEngine;

public class ShopContextHolder
{
    public ShopStateDTO CurrentShopState { get; private set; }

    public bool HasActiveShop => CurrentShopState != null;

    public void SaveState(ShopStateDTO state)
    {
        CurrentShopState = state;
        Debug.Log($"[ShopContextHolder] 已保存商店狀態: {state.ShopId}, 剩餘商品: {state.AvailableItems.Count} 件");
    }

    public void Clear()
    {
        CurrentShopState = null;
    }
}
