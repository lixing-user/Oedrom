using System;
using UnityEngine;
using Zenject;

// Application / PlayerResourceService.cs
public class PlayerResourceService : IResourceManager, IInitializable
{
    private readonly PlayerResource _playerResource;  // Domain Entity
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly SignalBus _signalBus;

    #region Initialize
    [Inject]
    public PlayerResourceService(
        PlayerResource playerResource,
        IDomainEventDispatcher eventDispatcher,
        SignalBus signalBus)
    {
        _playerResource = playerResource;
        _eventDispatcher = eventDispatcher;
        _signalBus = signalBus;
    }

    public void Initialize()
    {
        // 初始化資源
        InitializeVariables();
    }
    #endregion

    # region 通用方法
    public void InitializeVariables()
    {
        _playerResource.Reset();
       
        DispatchDomainEvents();  // 發布領域事件（由 Dispatcher 轉換為 Signal）
    }

    public void UpdateVariable(VariableType type, float amount)     //負責處理隨機事件的系統
    {
        switch (type)
        {
            case VariableType.morale:
                _playerResource.AdjustMorale(amount);
                break;
            case VariableType.popularity:
                _playerResource.AdjustPopularity(amount);
                break;
            case VariableType.supply:
                _playerResource.AdjustSupply(amount);
                break;
            case VariableType.coin:
                _playerResource.AdjustCoin(amount);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown resource type");
        }

        DispatchDomainEvents();
    }

    public float GetVariable(VariableType type)
    {
        return type switch
        {
            VariableType.morale => _playerResource.Morale,
            VariableType.popularity => _playerResource.Popularity,
            VariableType.supply => _playerResource.Supply,
            VariableType.coin => _playerResource.Coin,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown resource type")
        };
    }

    public float GetMaxValue(VariableType type)
    {
        return type switch
        {
            VariableType.morale => PlayerResource.MaxMorale,     
            VariableType.popularity => PlayerResource.MaxPopularity,
            VariableType.supply => PlayerResource.MaxSupply,
            VariableType.coin => PlayerResource.MaxCoin,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public int GetDynamicPrice(int basePrice)
    {
        return _playerResource.CalculateDynamicPrice(basePrice);
    }
    #endregion

    #region Use Cases

    #region 補給業務
    public void ApplyTurnStartEffects()
    {
        Debug.Log("[ResourceService] 執行回合開始效果...");

        // 執行 Domain 邏輯
        _playerResource.ApplyTurnEffects();

        // 發布領域事件
        DispatchDomainEvents();

        Debug.Log($"[ResourceService] 補給完成 - Supply: {_playerResource.Supply}");
    }

    // 扣除補給（EndPhase 使用）
    public bool DeductSupply(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[ResourceService] 無效的扣除數量: {amount}");
            return false;
        }

        float currentSupply = _playerResource.Supply;

        // === 業務規則：檢查是否足夠（可根據遊戲規則調整） ===
        if (currentSupply < amount)
        {
            Debug.LogWarning(
                $"[ResourceService] 補給不足！需要 {amount}，當前 {currentSupply}");

            // 不允許負數，觸發補給耗盡的特殊處理
            OnSupplyDepleted();
            return false;
        }

        // 正常扣除
        _playerResource.AdjustSupply(-amount);
        DispatchDomainEvents();

        Debug.Log($"[ResourceService] 扣除補給: -{amount}，剩餘: {_playerResource.Supply}");
        return true;
    }

    // 檢查補給是否足夠（不實際扣除）
    public bool CanAffordSupply(int amount)
    {
        return _playerResource.Supply >= amount;
    }

    // 補充補給（StandbyPhase 使用）
    public void ReplenishSupply(float amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[ResourceService] 無效的補充數量: {amount}");
            return;
        }

        _playerResource.AdjustSupply(amount);
        DispatchDomainEvents();

        Debug.Log($"[ResourceService] 補充補給: +{amount}，當前: {_playerResource.Supply}");
    }

    // 補給耗盡時的處理（業務邏輯）
    private void OnSupplyDepleted()
    {
        Debug.LogWarning("[ResourceService] 補給已耗盡！");

        // 可能的懲罰：
        // 1. 降低士氣
        //_playerResource.AdjustMorale(-1f);

        DispatchDomainEvents();
    }
    #endregion

    #region 金幣業務
    public bool TryPurchaseItem(int itemPrice)
    {
        // 計算實際價格（領域邏輯）
        int actualPrice = _playerResource.CalculateDynamicPrice(itemPrice);

        // 嘗試消費（領域邏輯）
        bool success = _playerResource.TrySpendCoin(actualPrice);

        if (success)
        {
            DispatchDomainEvents();
        }

        return success;
    }

    public void RewardCoin(float amount)
    {
        if (amount <= 0) return;
        _playerResource.AdjustCoin(amount);
        DispatchDomainEvents();
        Debug.Log($"[ResourceService] 獲得金幣: {amount}，目前金幣: {_playerResource.Coin}");
    }

    public bool TrySpendCoin(float amount)
    {
        if (amount <= 0) return false;
        bool success = _playerResource.TrySpendCoin(amount);
        if (success) DispatchDomainEvents();
        return success;
    }
    #endregion

    #region 軍心業務
    public void IncreaseeMorale(float amount)
    {
        _playerResource.AdjustMorale(amount);
        DispatchDomainEvents();
    }

    public void DecreaseMorale(float amount)
    {
        _playerResource.AdjustMorale(-amount);
        DispatchDomainEvents();
        Debug.Log($"[ResourceService] 軍心下降: {amount}");
    }
    #endregion

    #region 民心業務
    public void IncreasePopularity(float amount)
    {
        _playerResource.AdjustPopularity(amount);
        DispatchDomainEvents();
    }

    public void DecreasePopularity(float amount)
    {
        _playerResource.AdjustPopularity(-amount);
        DispatchDomainEvents();
        Debug.Log($"[ResourceService] 民心下降: {amount}");
    }
    #endregion

    #endregion

    #region 私有方法
    private void DispatchDomainEvents()
    {
        var events = _playerResource.DomainEvents;
        if (events.Count > 0)
        {
            _eventDispatcher.Dispatch(events);
            _playerResource.ClearDomainEvents();
        }
    }
    #endregion
}
