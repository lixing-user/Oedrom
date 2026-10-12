using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.STP;

/// <summary>
/// 事件效果工廠（Infrastructure → Domain 轉換）
/// </summary>
public class EventEffectFactory
{
    // --- 核心：效果創建器註冊表 ---
    private readonly Dictionary<EventEffectType, Func<EventEffectConfig, IEventEffect>> _effectCreators;
    private readonly Dictionary<VariableType, Func<float, IEventEffect>> _resourceChangeCreators;
    private readonly Dictionary<VariableType, Func<EventEffectConfig, IEventEffect>> _resourceModifierCreators;


    public EventEffectFactory()
    {
        // 1. 註冊：即時資源變化 (ResourceChange)
        _resourceChangeCreators = new Dictionary<VariableType, Func<float, IEventEffect>>
        {
            { VariableType.morale,     amount => new MoraleChangeEffect(amount) },
            { VariableType.popularity, amount => new PopularityChangeEffect(amount) },
            { VariableType.supply,     amount => new SupplyChangeEffect(amount) },
            { VariableType.coin,       amount => new CoinChangeEffect(amount) }
        };


        // 2. 註冊：持續性資源修改 (ResourceModifier)
        _resourceModifierCreators = new Dictionary<VariableType, Func<EventEffectConfig, IEventEffect>>
        {
            { VariableType.supply, config => new SupplyBoostEffect(config.flatAmount, config.percentAmount, config.duration) }
            // 未來擴充例如：{ VariableType.coin, config => new CoinBoostEffect(...) }
        };


        // 3. 註冊：主效果分發 (Main Effect Routing)
        _effectCreators = new Dictionary<EventEffectType, Func<EventEffectConfig, IEventEffect>>
        {
            { EventEffectType.ResourceChange,        CreateResourceChangeEffect },
            { EventEffectType.ResourceModifier,      CreateResourceModifierEffect },
            
            // 獨立的複雜 Effect 可以直接在這裡 new
            { EventEffectType.DynamicResourceChange, config => new DynamicResourceChangeEffect(
                config.targetResource, config.sourceResource, config.flatAmount, config.percentAmount) },

            { EventEffectType.StatusEffect, config => new ApplyStatusEffect(config.duration) },

            { EventEffectType.DeckEffect, config => new AddCardEffect(config.targetPieceType, config.targetTeam, (int)config.flatAmount, config.randomPool.pieces) },

            { EventEffectType.CombatModifier,config => new ApplyCombatModifierEffect(config.combatModifierType, config.duration)},

            { EventEffectType.TriggerBattle,config => new TriggerBattleEffect(config.targetBattleType, config.targetBattleId)}
            // 未來擴充例如：
            // { EventEffectType.TriggerBattle, config => new TriggerBattleEffect() },
        };
    }

    #region 核心邏輯
    /// <summary>
    /// 從配置創建效果實例
    /// </summary>
    public List<IEventEffect> CreateEffects(List<EventEffectConfig> effectConfigs)
    {
        var effects = new List<IEventEffect>();

        foreach (var config in effectConfigs)
        {
            IEventEffect effect = CreateEffect(config);
            if (effect != null)
            {
                effects.Add(effect);
            }
        }

        return effects;
    }

    private IEventEffect CreateEffect(EventEffectConfig config)
    {
        if (_effectCreators.TryGetValue(config.effectType, out var creator))
        {
            return creator.Invoke(config);
        }

        Debug.LogWarning($"[EventEffectFactory] 未註冊的效果類型: {config.effectType}");
        return null;
    }
    #endregion

    #region 業務實作
    private IEventEffect CreateResourceChangeEffect(EventEffectConfig config)
    {
        if (_resourceChangeCreators.TryGetValue(config.targetResource, out var creator))
        {
            return creator.Invoke(config.flatAmount);
        }

        Debug.LogWarning($"[EventEffectFactory] 未註冊的資源改變類型: {config.targetResource}");
        return null;
    }


    private IEventEffect CreateResourceModifierEffect(EventEffectConfig config)
    {
        if (config.duration <= 1)
        {
            Debug.LogWarning($"[EventEffectFactory] ResourceModifier 的 duration 應該 > 1，當前為 {config.duration}");
        }

        if (_resourceModifierCreators.TryGetValue(config.targetResource, out var creator))
        {
            return creator.Invoke(config);
        }

        Debug.LogWarning($"[EventEffectFactory] 未註冊的持續資源修改類型: {config.targetResource}");
        return null;
    }
    #endregion
}
