using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Domain Layer: Represents player resources in the game.
public class PlayerResource
{
    #region 屬性
    // === 私有狀態（封裝內部數據） ===
    private float morale;
    private float popularity;
    private float supply;
    private float coin;

    // === 領域常量（業務規則的一部分） ===
    private const float maxMorale = 10f;
    private const float maxPopularity = 10f;
    private const float maxSupply = 500f;
    private const float maxCoin = 10000f;
    private const float minValue = 0f;

    private const float DefaultMorale = 5f;
    private const float DefaultPopularity = 5f;
    private const float DefaultSupply = 500f;
    private const float DefaultCoin = 5000f;

    private const float DefaultSupplyReplenishment = 50f;

    // === 只讀屬性（查詢狀態，不暴露 setter） ===
    public float Morale => morale;
    public float Popularity => popularity;
    public float Supply => supply;
    public float Coin => coin;
    public static float MaxMorale => maxMorale;
    public static float MaxPopularity => maxPopularity;
    public static float MaxSupply => maxSupply;
    public static float MaxCoin => maxCoin;
    #endregion

    #region 領域事件
    // 領域事件列表
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    // 活躍的效果列表
    private readonly List<ResourceModifier> _activeModifiers = new();
    public IReadOnlyList<ResourceModifier> ActiveModifiers => _activeModifiers.AsReadOnly();

    #endregion

    #region 建構函式
    // === 建構函式（強制執行不變量） ===
    public PlayerResource()
    {
        morale = DefaultMorale;
        popularity = DefaultPopularity;
        supply = DefaultSupply;
        coin = DefaultCoin;

        AddDomainEvent(new ResourceInitializedDomainEvent(
           morale, popularity, supply, coin));
    }
    public PlayerResource(float morale, float popularity, float supply, float coin)        // 即使從外部載入，也要確保符合不變量
    {
        this.morale = Mathf.Clamp(morale, minValue, MaxMorale);
        this.popularity = Mathf.Clamp(popularity, minValue, MaxPopularity);
        this.supply = Mathf.Clamp(supply, minValue, MaxSupply);
        this.coin = Mathf.Clamp(coin, minValue, MaxCoin);
    }
    #endregion

    #region 業務行為方法(調整資源數值)
    public void AdjustMorale(float amount)
    {
        float oldValue = morale;
        morale = Mathf.Clamp(morale + amount, minValue, MaxMorale);

        if (Math.Abs(morale - oldValue) > 0.001f)
        {
            AddDomainEvent(new ResourceChangedDomainEvent(
                VariableType.morale, morale, oldValue));
        }
    }

    public void AdjustPopularity(float amount)
    {
        float oldValue = popularity;
        popularity = Mathf.Clamp(popularity + amount, minValue, MaxPopularity);

        if (Math.Abs(popularity - oldValue) > 0.001f)
        {
            AddDomainEvent(new ResourceChangedDomainEvent(
                VariableType.popularity, popularity, oldValue));

            // 人氣變化是領域邏輯的一部分，應該觸發相關事件
            AddDomainEvent(new PopularityChangedDomainEvent(popularity));
        }
    }

    public void AdjustSupply(float amount)
    {
        float oldValue = supply;
        supply = Mathf.Clamp(supply + amount, minValue, MaxSupply);

        if (Math.Abs(supply - oldValue) > 0.001f)
        {
            AddDomainEvent(new ResourceChangedDomainEvent(
                VariableType.supply, supply, oldValue));

            // 補給耗盡是重要的領域事件
            if (supply <= 0)
            {
                AddDomainEvent(new SupplyDepletedDomainEvent());
            }
        }
    }

    public void AdjustCoin(float amount)
    {
        float oldValue = coin;
        coin = Mathf.Clamp(coin + amount, minValue, MaxCoin);

        if (Math.Abs(coin - oldValue) > 0.001f)
        {
            AddDomainEvent(new ResourceChangedDomainEvent(
                VariableType.coin, coin, oldValue));
        }
    }
    #endregion

    #region 業務行為方法(外部驗證)
    public void Reset()
    {
        morale = DefaultMorale;
        popularity = DefaultPopularity;
        supply = DefaultSupply;
        coin = DefaultCoin;

        AddDomainEvent(new ResourceResetDomainEvent(morale, popularity, supply, coin));
    }

    public static float GetMaxValue(VariableType type)
    {
        return type switch
        {
            VariableType.morale => MaxMorale,
            VariableType.popularity => MaxPopularity,
            VariableType.supply => MaxSupply,
            VariableType.coin => MaxCoin,
            _ => 0f
        };
    }


    /// <summary>
    /// 套用回合開始效果（包含事件影響）
    /// </summary>
    public void ApplyTurnEffects()
    {
        // 更新效果持續時間
        UpdateModifiers();

        // === 計算受事件影響的補給數值 ===
        float baseReplenishment = DefaultSupplyReplenishment;
        float finalReplenishment = CalculateModifiedValue(
            VariableType.supply,
            baseReplenishment);

        // 每回合自動增加補給是遊戲規則
        AdjustSupply(finalReplenishment);

        AddDomainEvent(new TurnEffectsAppliedDomainEvent());
    }

    /// <summary>
    /// 計算受效果影響後的數值（Domain 邏輯）
    /// </summary>
    private float CalculateModifiedValue(VariableType resourceType, float baseValue)
    {
        var applicableModifiers = _activeModifiers
            .Where(m => m.TargetResource == resourceType)
            .ToList();

        if (applicableModifiers.Count == 0)
            return baseValue;

        // 方案 A：疊加計算
        float result = baseValue;
        foreach (var modifier in applicableModifiers)
        {
            result = modifier.Apply(result);
        }

        // 方案 B：先加總固定值，再乘總百分比
        // float totalFlat = applicableModifiers.Sum(m => m.FlatAmount);
        // float totalPercent = applicableModifiers.Sum(m => m.PercentAmount);
        // result = (baseValue + totalFlat) * (1f + totalPercent);

        return result;
    }


    public bool HasEnoughCoin(float requiredAmount)
    {
        return coin >= requiredAmount;
    }

    public bool TrySpendCoin(float amount)
    {
        if (!HasEnoughCoin(amount))
            return false;

        AdjustCoin(-amount);
        return true;
    }

    public int CalculateDynamicPrice(int basePrice)
    {
        // 業務規則：人氣每增加 1，獲得 5% 折扣，最多 50% 折扣
        float discount = Mathf.Clamp(1f - 0.05f * popularity, 0.5f, 1f);
        return Mathf.RoundToInt(basePrice * discount);
    }

    #endregion

    #region 效果管理
    /// <summary>
    /// 添加效果（Buff/Debuff）
    /// </summary>
    public void ApplyModifier(ResourceModifier modifier)
    {
        _activeModifiers.Add(modifier);

        AddDomainEvent(new ModifierAppliedDomainEvent(
            modifier.EffectId,
            modifier.TargetResource,
            modifier.RemainingTurns));
    }

    /// <summary>
    /// 移除效果
    /// </summary>
    public void RemoveModifier(string effectId)
    {
        var modifier = _activeModifiers.FirstOrDefault(m => m.EffectId == effectId);
        if (modifier != null)
        {
            _activeModifiers.Remove(modifier);
            AddDomainEvent(new ModifierRemovedDomainEvent(effectId));
        }
    }

    /// <summary>
    /// 更新所有效果的持續時間
    /// </summary>
    private void UpdateModifiers()
    {
        // 減少持續時間
        foreach (var modifier in _activeModifiers)
        {
            modifier.DecrementDuration();
        }

        // 移除過期效果
        _activeModifiers.RemoveAll(m => m.IsExpired);
    }
    #endregion
}