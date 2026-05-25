using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Shop3DView : MonoBehaviour
{
    [Header("Buy Area")]
    [SerializeField] private Transform[] ItemPedestals;

    [Header("Sell Area")]
    [SerializeField] private Collider _sellAreaBounds;

    [Header("Repair Area")]
    [SerializeField] private Collider _repairAreaBounds;

    [Inject] private ShopItemSpawner _itemSpawner;

    public event System.Action<string> OnItem3DClicked;
    private Dictionary<string, GameObject> _spawned3DItems = new Dictionary<string, GameObject>();

    private List<GameObject> _pendingSell3DItems = new List<GameObject>();

    private List<GameObject> _pendingRepair3DItems = new List<GameObject>();

    #region Buy Area 視覺邏輯
    public void Spawn3DItems(List<ShopItemDisplayData> displayItems)
    {
        for (int i = 0; i < displayItems.Count && i < ItemPedestals.Length; i++)
        {
            var data = displayItems[i];
            var pedestal = ItemPedestals[i];

            // 1. 直接實體化專屬的 Prefab
            GameObject itemObj = _itemSpawner.Spawn(data, pedestal);

            // 2. 掛上點擊事件的 Proxy，確保能被玩家點擊
            // 記得確保你的 previewPrefab 身上有 Collider，否則點擊無效
            var clickableProxy = itemObj.AddComponent<ShopItem3DClickProxy>();
            clickableProxy.ItemId = data.ItemId;
            clickableProxy.OnClicked += (id) => OnItem3DClicked?.Invoke(id);

            _spawned3DItems.Add(data.ItemId, itemObj);
        }
    }

    public void Remove3DItem(string itemId)
    {
        if (_spawned3DItems.TryGetValue(itemId, out GameObject obj))
        {
            Destroy(obj);
            _spawned3DItems.Remove(itemId);
        }
    }
    #endregion

    #region Sell Area 視覺邏輯
    public void SpawnPendingSellItem(ChessPieceType pieceType)
    {
        if (_sellAreaBounds == null)
        {
            Debug.LogError("[Shop3DView] 尚未綁定 SellAreaBounds!");
            return;
        }

        Bounds bounds = _sellAreaBounds.bounds;

        // 1. 空間內隨機座標 (Y軸也給予一定範圍，讓模型像是堆在空間裡)
        Vector3 randomPos = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );

        // 2. 三軸隨機旋轉，讓棋子呈現隨意散落的感覺
        Quaternion randomRot = Quaternion.Euler(
            Random.Range(0, 0f),
            Random.Range(0, 0f),
            Random.Range(0, 0f)
        );

        // 3. 呼叫更新後的 Spawner 生成
        GameObject itemObj = _itemSpawner.SpawnSellPreview(pieceType, randomPos, randomRot);
        Debug.Log($"[Shop3DView] Spawned pending sell item: {pieceType} at {randomPos} with rotation {randomRot.eulerAngles}");

        if (itemObj != null)
        {
            _pendingSell3DItems.Add(itemObj);
        }
    }

    public void ClearPendingSellItems()
    {
        foreach (var item in _pendingSell3DItems)
        {
            if (item != null) Destroy(item);
        }
        _pendingSell3DItems.Clear();
    }
    #endregion

    #region Repair Area 視覺邏輯
    public void SpawnPendingRepairItem(ChessPieceType pieceType)
    {
        if (_repairAreaBounds == null)
        {
            Debug.LogError("[Shop3DView] 尚未綁定 RepairAreaBounds!");
            return;
        }

        Bounds bounds = _repairAreaBounds.bounds;

        // 1. 空間內隨機座標 (Y軸也給予一定範圍，讓模型像是堆在空間裡)
        Vector3 randomPos = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );

        // 2. 三軸隨機旋轉，讓棋子呈現隨意散落的感覺
        Quaternion randomRot = Quaternion.Euler(
            Random.Range(0, 0f),
            Random.Range(0, 0f),
            Random.Range(0, 0f)
        );

        // 3. 呼叫更新後的 Spawner 生成
        GameObject itemObj = _itemSpawner.SpawnSellPreview(pieceType, randomPos, randomRot);
        Debug.Log($"[Shop3DView] Spawned pending sell item: {pieceType} at {randomPos} with rotation {randomRot.eulerAngles}");

        if (itemObj != null)
        {
            _pendingSell3DItems.Add(itemObj);
        }
    }

    public void ClearPendingRepairItems()
    {
        foreach (var item in _pendingRepair3DItems)
        {
            if (item != null) Destroy(item);
        }
        _pendingRepair3DItems.Clear();
    }
    #endregion
}
