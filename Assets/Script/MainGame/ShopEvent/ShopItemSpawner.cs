using UnityEngine;
using Zenject;
using ChessPieceCardProduction;

public class ShopItemSpawner
{
    [Inject] private DiContainer _container;
    [Inject] private ICardConfigProvider _cardConfigProvider;

    #region 買入區生成 (貨架)
    public GameObject Spawn(ShopItemDisplayData data, Transform parent)
    {
        GameObject prefabToSpawn = null;
        // 1. 向 Provider 索取對應的 3D Prefab
        if (data.ItemType == ShopItemType.Piece && data.PieceType.HasValue)
        {
            if (_cardConfigProvider.TryGetConfig(data.PieceType.Value, out CardConfig config))
            {
                prefabToSpawn = config.previewPrefab; // 取得預覽模型
            }
        }
        else if (data.ItemType == ShopItemType.Supply)
        {
            // 未來如果是補給品，就在這裡實作抓取補給品 Prefab 的邏輯
        }

        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"無法找到對應的商品模型: {data.ItemId}");
            return null;
        }

        // 2. 生成實體
        GameObject spawnedObj = _container.InstantiatePrefab(prefabToSpawn, parent.position, parent.rotation, parent);
        // 3. 設定 Layer 為 ShopItem，確保 InputHandler 能偵測到
        SetLayerRecursively(spawnedObj, LayerMask.NameToLayer("ShopItem"));

        Debug.Log("[ShopItemSpawner]已生成商品prefab");
        return spawnedObj;
    }
    #endregion

    #region 賣出區生成 (隨機空間)
    public GameObject SpawnSellPreview(ChessPieceType pieceType, Vector3 position, Quaternion rotation)
    {
        if (!_cardConfigProvider.TryGetConfig(pieceType, out CardConfig config))
        {
            Debug.LogWarning($"[ShopItemSpawner] 無法找到賣出預覽的商品模型: {pieceType}");
            return null;
        }

        // 生成實體 (這裡不需要 parent，直接放在世界空間即可)
        GameObject spawnedObj = _container.InstantiatePrefab(config.previewPrefab, position, rotation, null);

        // 賣出區的預覽模型不需要被玩家再次點擊
        SetLayerRecursively(spawnedObj, LayerMask.NameToLayer("ShopItem"));

        var colliders = spawnedObj.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        var rigidbodies = spawnedObj.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = true;
        }

        return spawnedObj;
    }
    #endregion

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
