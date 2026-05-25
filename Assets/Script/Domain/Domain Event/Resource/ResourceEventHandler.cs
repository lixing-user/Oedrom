using System;
using UnityEngine;
using Zenject;

public class ResourceEventHandler :
    IEventHandler<ResourceChangedDomainEvent>,
    IEventHandler<ResourceResetDomainEvent>,
    IEventHandler<PopularityChangedDomainEvent>,
    IEventHandler<ResourceInitializedDomainEvent>,
    IEventHandler<SupplyDepletedDomainEvent>,
    IEventHandler<TurnEffectsAppliedDomainEvent>,
    IInitializable,
    IDisposable
{
    private readonly IEventHandlerRegistry _registry;
    private readonly SignalBus _signalBus;

    public ResourceEventHandler(
       IEventHandlerRegistry registry,
       SignalBus signalBus)
    {
        _registry = registry;
        _signalBus = signalBus;
    }

    #region Initialize & Dispose
    public void Initialize()
    {
        _registry.Register<ResourceChangedDomainEvent>(this);
        _registry.Register<ResourceResetDomainEvent>(this);
        _registry.Register<PopularityChangedDomainEvent>(this);
        _registry.Register<ResourceInitializedDomainEvent>(this);
        _registry.Register<SupplyDepletedDomainEvent>(this);
        _registry.Register<TurnEffectsAppliedDomainEvent>(this);

        Debug.Log("[ResourceEventHandler] 資源事件處理器已註冊");
    }

    public void Dispose()
    {
        _registry.Unregister<ResourceChangedDomainEvent>(this);
        _registry.Unregister<ResourceResetDomainEvent>(this);
        _registry.Unregister<PopularityChangedDomainEvent>(this);
        _registry.Unregister<ResourceInitializedDomainEvent>(this);
        _registry.Unregister<SupplyDepletedDomainEvent>(this);
        _registry.Unregister<TurnEffectsAppliedDomainEvent>(this);

        Debug.Log("[ResourceEventHandler] 資源事件處理器已移除");
    }
    #endregion

    #region 事件處理方法
    public void Handle(ResourceInitializedDomainEvent @event)
    {
        // 批次發送所有資源的 UI 更新信號
        _signalBus.Fire(new ResourceChangedSignal(VariableType.morale, @event.Morale));
        _signalBus.Fire(new ResourceChangedSignal(VariableType.popularity, @event.Popularity));
        _signalBus.Fire(new ResourceChangedSignal(VariableType.supply, @event.Supply));
        _signalBus.Fire(new ResourceChangedSignal(VariableType.coin, @event.Coin));

        Debug.Log($"[ResourceEventHandler] 資源初始化完成 - Morale:{@event.Morale}, Popularity:{@event.Popularity}, Supply:{@event.Supply}, Coin:{@event.Coin}");
    }

    public void Handle(ResourceChangedDomainEvent @event)
    {
        _signalBus.Fire(new ResourceChangedSignal(
            @event.Type,
            @event.NewValue
        ));

        Debug.Log($"[ResourceEventHandler] 資源變更: {@event.Type} = {@event.NewValue} (舊值: {@event.OldValue})");
    }

    public void Handle(ResourceResetDomainEvent @event)
    {
        _signalBus.Fire(new ResourceChangedSignal(VariableType.morale, @event.DefaultMorale));
        _signalBus.Fire(new ResourceChangedSignal(VariableType.popularity, @event.DefaultPopularity));
        _signalBus.Fire(new ResourceChangedSignal(VariableType.supply, @event.DefaultSupply));
        _signalBus.Fire(new ResourceChangedSignal(VariableType.coin, @event.DefaultCoin));
    }

    public void Handle(PopularityChangedDomainEvent @event)
    {
        // 跨聚合協調邏輯（在 Infrastructure 處理）
        int difficulty = Mathf.RoundToInt(@event.NewPopularity);
        _signalBus.Fire(new EnemyDifficultyDeterminedSignal(difficulty));

        Debug.Log($"[ResourceEventHandler] 人氣變化 → 敵人難度調整為: {difficulty}");
    }

    public void Handle(SupplyDepletedDomainEvent @event)
    {
        // 觸發特殊的警告 UI 或音效
        _signalBus.Fire(new SupplyDepletedWarningSignal());

        Debug.LogWarning($"[ResourceEventHandler] 補給已耗盡！時間: {@event.OccurredAt}");
    }

    public void Handle(TurnEffectsAppliedDomainEvent @event)
    {
        // 可以用來記錄日誌或觸發額外的視覺效果
        Debug.Log($"[ResourceEventHandler] 回合效果已應用於: {@event.OccurredAt}");
    }
    #endregion
}

