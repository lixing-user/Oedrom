using System;
using System.Linq;
using UnityEngine;
using Zenject;

public class LootResolutionService : IInitializable, IDisposable
{
    [Inject] private SignalBus _signalBus;
    [Inject] private BattleContextHolder _battleContext;
    [Inject] private ShopContextHolder _shopContext;

    [Inject] private IHandService _handService;
    [Inject] private ICardDataFactory _cardFactory;
    [Inject] private IResourceManager _playerResourceService;

    public void Initialize()
    {
        _signalBus.Subscribe<BattleResolveSignal>(OnBattleResolved);
    }
    public void Dispose()
    {
        _signalBus.TryUnsubscribe<BattleResolveSignal>(OnBattleResolved);
    }

    private void OnBattleResolved(BattleResolveSignal signal)
    {
        if (signal.BattleType != BattleType.Loot) return;

        var battleData = _battleContext.CurrentBattle;
        bool isPlayerWin = signal.WinnerTeam == (int)Team.White;

        if (isPlayerWin)
        {
            Debug.Log("[LootResolution] 搶劫成功！發放獎勵並洗劫商店...");

            if (_shopContext.HasActiveShop)
            {
                var shopItems = _shopContext.CurrentShopState.AvailableItems;

                // 1. 發放戰鬥前紀錄的商品 (TargetItemIds)
                foreach (var itemId in battleData.TargetItemId)
                {
                    // 從跨場景的 DTO 中把該商品的詳細資料找出來
                    var itemData = shopItems.FirstOrDefault(i => i.Id == itemId);
                    if (itemData == null) continue;

                    // 轉化並發放獎勵 (邏輯與 TryBuyItem 相同)
                    if (itemData.ItemType == ShopItemType.Piece && itemData.PieceType.HasValue)
                    {
                        var newCard = _cardFactory.CreateCard(itemData.PieceType.Value, Team.White);
                        _handService.AddCard(newCard);
                        Debug.Log($"[LootResolution] 搶奪成功，獲得卡牌: {itemData.PieceType}");
                    }
                    else if (itemData.ItemType == ShopItemType.Supply)
                    {
                        _playerResourceService.ReplenishSupply(itemData.SupplyAmount);
                        Debug.Log($"[LootResolution] 搶奪成功，獲得補給: {itemData.SupplyAmount}");
                    }
                }
            }
        }
        else
        {
            Debug.Log("[LootResolution] 搶劫失敗!");
            // 處理失敗懲罰
            _playerResourceService.DecreaseMorale(1f);
            _playerResourceService.DeductSupply(20);
        }

        if (_shopContext.HasActiveShop)
        {
            _shopContext.CurrentShopState.AvailableItems.Clear(); // 架上清空
            _shopContext.CurrentShopState.IsLooted = true;        // 標記已被搶劫，Presenter 會據此封鎖 UI
        }
    }
}
