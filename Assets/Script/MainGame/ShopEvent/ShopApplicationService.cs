using ChessPieceCardProduction;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

// Application/Services/ShopApplicationService.cs
public class ShopApplicationService : IShopService//, IInitializable
{
    private readonly SignalBus _signalBus;
    private readonly Shop _shop;
    private readonly ShopContextHolder _shopContextHolder;

    private readonly IResourceManager _playerResourceService;
    private readonly IHandService _handService;
    private readonly ICardDataFactory _cardFactory;
    private readonly IShopFactory _shopFactory;

    public ShopApplicationService(
        Shop shop,
        ShopContextHolder shopContextHolder,
        IResourceManager playerResource,
        IHandService handService,
        ICardDataFactory cardFactory,
        SignalBus signalBus,
        IShopFactory shopFactory)
    {
        _shop = shop;
        _shopContextHolder = shopContextHolder;
        _playerResourceService = playerResource;
        _handService = handService;
        _cardFactory = cardFactory;
        _signalBus = signalBus;
        _shopFactory = shopFactory;
    }

    #region 商店生命週期
    public void Initialize()
    {
        if (_shopContextHolder.HasActiveShop)
        {
            RestoreShopFromDTO(_shopContextHolder.CurrentShopState);
        }
        else
        {
            RefreshShopInventory();
        }
    }

    public void ClearShopState()
    {
        _shopContextHolder.Clear();
        Debug.Log("[ShopApplicationService] 已清空商店暫存狀態，下次進入將重新生成商品。");
    }

    public void RefreshShopInventory()
    {
        // 1. 呼叫工廠生成新的商品清單
        var newItems = _shopFactory.GenerateRandomShopItems();

        // 2. 將商品上架到 Shop 聚合根中
        _shop.Restock(newItems);

        // 3. 建立這間新商店的 DTO 並保存到全局 Holder 中
        var newState = new ShopStateDTO
        {
            ShopId = System.Guid.NewGuid().ToString(),
            AvailableItems = new List<ShopItemData>(newItems) // 複製一份存起來
        };
        _shopContextHolder.SaveState(newState);

        Debug.Log("[ShopApplicationService] 商店已完成隨機進貨！");
    }

    private void RestoreShopFromDTO(ShopStateDTO dto)
    {
        _shop.Restock(dto.AvailableItems);
        Debug.Log($"[ShopApplicationService] 從暫存恢復商店狀態！ShopID: {dto.ShopId}");
    }
    #endregion

    #region 查詢邏輯
    public IReadOnlyList<ShopItemData> GetAvailableShopItems()
    {
        // 也可以在這裡加入過濾邏輯，例如「只回傳玩家買得起的」或「目前尚未解鎖的隱藏」
        return _shop.GetAllItems();
    }

    public int GetCardSellValue(CardID cardId)
    {
        var card = _handService.GetHandCards().FirstOrDefault(c => c.CardId == cardId);
        return card != null ? card.CalculateSellValue() : 0;
    }

    public bool IsShopLooted()
    {
        return _shopContextHolder.HasActiveShop && _shopContextHolder.CurrentShopState.IsLooted;
    }
    #endregion

    #region 買入邏輯
    public bool TryBuyItem(string itemId, Team playerTeam = Team.White)
    {
        // 1. 取得商品資訊 (向 Shop 查詢)
        var item = _shop.GetItem(itemId);
        if (item == null) return false;

        // 2. 計算動態價格 (向 PlayerResource 查詢)
        if (!_playerResourceService.TryPurchaseItem(item.Cost))
        {
            return false;
        }

        // 4. 根據商品類型處理 (轉化為卡牌或補給)
        if (item.ItemType == ShopItemType.Piece)
        {
            HandCard newCard = _cardFactory.CreateCard(item.PieceType.Value, playerTeam);   // 委派給 Factory 製造卡牌
            _handService.AddCard(newCard);
        }
        else if (item.ItemType == ShopItemType.Supply)
        {
            _playerResourceService.ReplenishSupply(item.SupplyAmount);
        }

        // 5. 完成交易，將商品從商店下架 
        _shop.RemoveItem(itemId);

        // 6. 同步更新跨場景的 DTO
        if (_shopContextHolder.HasActiveShop)
        {
            var itemInDto = _shopContextHolder.CurrentShopState.AvailableItems.FirstOrDefault(i => i.Id == itemId);
            if (itemInDto != null)
            {
                _shopContextHolder.CurrentShopState.AvailableItems.Remove(itemInDto);
            }
        }

        return true;
    }
    #endregion

    #region 賣出邏輯
    public bool TrySellCards(List<CardID> cardIds)
    {
        if (cardIds == null || cardIds.Count == 0) return false;

        int totalSellValue = 0;
        List<CardID> successfullySold = new List<CardID>();

        foreach (var cardId in cardIds)
        {
            // 1. 檢查玩家有沒有這張卡
            var card = _handService.GetHandCards().FirstOrDefault(c => c.CardId == cardId);
            if (card != null)
            {
                // 2. 計算賣出價值
                totalSellValue += card.CalculateSellValue();

                // 3. 移除卡牌
                _handService.RemoveCard(cardId, RemovalReason.Sold);
                successfullySold.Add(cardId);
            }
        }

        if (successfullySold.Count > 0)
        {
            _playerResourceService.UpdateVariable(VariableType.coin, totalSellValue);
            Debug.Log($"[ShopService] 成功賣出 {successfullySold.Count} 個棋子，獲得 {totalSellValue} 金幣");
            return true;
        }

        return false;
    }
    #endregion

    #region 卡牌修復邏輯
    // 查詢單卡修復費用 (供 Presenter 顯示金額)
    public int GetCardRepairCost(CardID cardId)
    {
        var card = _handService.GetHandCards().FirstOrDefault(c => c.CardId == cardId);
        if (card == null || card.GetStatus() != CardStatus.broken) return 0;

        // 制定修復費用的 Domain Rule，例如為基礎費用的 80%，或者其他公式
        return (int)System.Math.Round(card.GetTotalCost() * 0.8f);
    }

    public bool TryRepairCard(List<CardID> cardIds)
    {
        // 1. 計算總修復費用
        int totalCost = 0;
        foreach (var id in cardIds)
        {
            totalCost += GetCardRepairCost(id);
        }

        // 2. 檢查玩家資源 (請替換為您專案中對應的金幣變數名稱)
        float currentGold = _playerResourceService.GetVariable(VariableType.coin); // 假設有 gold
        if (currentGold < totalCost)
        {
            Debug.LogWarning("[ShopService] 金幣不足，無法修復");
            return false;
        }

        // 3. 扣除資源
        _playerResourceService.TrySpendCoin(totalCost);

        // 4. 對每張卡牌進行修復
        foreach (var id in cardIds)
        {
            _handService.RepairCard(id);
        }

        Debug.Log($"[ShopService] 成功修復 {cardIds.Count} 張卡牌，花費 {totalCost}");
        return true;
    }
    #endregion

    #region 觸發搶劫/戰鬥邏輯
    public bool TryTriggerLootBattle(CardID triggerCardId)
    {
        if (IsShopLooted()) return false;
        
        // 1. 檢查玩家是否真的有這張卡
        var card = _handService.GetHandCards().FirstOrDefault(c => c.CardId == triggerCardId);
        if (card == null) return false;

        // 2. 扣除人氣值 (Popularity 下降)
        _playerResourceService.DecreasePopularity(1);

        // 3. 獲取當前商店內的所有商品，這就是我們的「搶劫目標」
        var allItemsInShop = _shop.GetAllItems();
        var targetIds = allItemsInShop.Select(item => item.Id).ToList();

        // 4. 打包戰鬥資料 (Unified Context)
        var sessionData = new BattleSessionData
        {
            BattleId = System.Guid.NewGuid().ToString(),
            BattleType = BattleType.Loot,
            // 共用資料
            AttackerType = card.CardType,
            AttackerTeam = Team.White, // 發起搶劫的固定是玩家(白方)
            // 搶劫專用資料
            AttackerCard = card,
            TargetItemId = targetIds // 假設你的 Service 或 Presenter 有存這個值
        };

        // 5. 觸發切換場景事件
        Debug.Log($"玩家使用 {card.CardType} 觸發了戰鬥！");
        _signalBus.Fire(new RequestEnterBattleSignal
        {
            BattleType = BattleType.Loot,
            PendingBattle = sessionData
        });

        return true;
    }
    #endregion
}

public interface IShopService
{
    void RefreshShopInventory();
    void ClearShopState();
    IReadOnlyList<ShopItemData> GetAvailableShopItems();        // 提供 UI 獲取商品列表
    int GetCardSellValue(CardID cardId);
    int GetCardRepairCost(CardID cardId);
    bool IsShopLooted();
    bool TryBuyItem(string itemId, Team playerTeam = Team.White);
    bool TrySellCards(List<CardID> cardIds);
    bool TryRepairCard(List<CardID> cardIds);
    bool TryTriggerLootBattle(CardID triggerCardId);
}