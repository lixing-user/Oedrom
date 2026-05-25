using System.Collections.Generic;
using UnityEngine;
using Zenject;

// Application/EventSystem/EventEffectDispatcher.cs
public class EventEffectDispatcher
{
    private readonly SignalBus _signalBus;
    private readonly EventEffectFactory _effectFactory;
    private readonly EffectContextFactory _contextFactory;

    public EventEffectDispatcher(
        SignalBus signalBus,
        EventEffectFactory effectFactory,
        EffectContextFactory contextFactory)
    {
        _signalBus = signalBus;
        _effectFactory = effectFactory;
        _contextFactory = contextFactory;
    }


    /// <summary>
    /// 分發事件效果
    /// </summary>
    public void Dispatch(List<EventEffectConfig> effectConfigs, PieceID targetPieceId)
    {
        // 1. 從配置創建效果實例
        var effects = _effectFactory.CreateEffects(effectConfigs);

        // 2. 創建執行上下文
        var context = _contextFactory.Create(targetPieceId);

        // 3. 執行所有效果
        foreach (var effect in effects)
        {
            if (effect.CanExecute(context))
            {
                effect.Execute(context);
                Debug.Log($"[EventEffectDispatcher] 執行事件: {effect.EffectId}");
            }
            else
            {
                Debug.LogWarning($"[EventEffectDispatcher] 效果 {effect.EffectId} 無法執行（前置條件不滿足）");
            }
        }

        Debug.Log($"[EventEffectDispatcher] 事件執行完成，共執行 {effects.Count} 個效果");
    }
}

