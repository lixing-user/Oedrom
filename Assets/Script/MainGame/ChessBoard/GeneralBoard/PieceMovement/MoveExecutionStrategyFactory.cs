using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IMoveExecutionStrategyFactory
{
    IMoveExecutionStrategy GetStrategy(MoveExecutionMode mode);
}

public enum MoveExecutionMode
{
    Direct,
    Preview,
    Swap,
    Setup
}


public class MoveExecutionStrategyFactory : IMoveExecutionStrategyFactory
{
    private readonly DiContainer _container;
    private readonly Dictionary<MoveExecutionMode, Type> _strategyTypes;
    private readonly Dictionary<MoveExecutionMode, IMoveExecutionStrategy> _cachedStrategies;

    public MoveExecutionStrategyFactory (DiContainer container)
    {
        _container = container;
        _cachedStrategies = new Dictionary<MoveExecutionMode, IMoveExecutionStrategy>();

        // 映射模式到實現類型
        _strategyTypes = new Dictionary<MoveExecutionMode, Type>
        {
            { MoveExecutionMode.Direct, typeof(DirectMoveStrategy) },
            { MoveExecutionMode.Preview, typeof(PreviewMoveStrategy) },
            { MoveExecutionMode.Swap, typeof(SwapMoveStrategy) },
            { MoveExecutionMode.Setup, typeof(SetupMoveStrategy) }
        };
    }

    public IMoveExecutionStrategy GetStrategy(MoveExecutionMode mode)
    {
        if (_cachedStrategies.TryGetValue(mode, out var cachedStrategy))
            return cachedStrategy;

        // 嘗試從容器 resolve
        if (_strategyTypes.TryGetValue(mode, out var strategyType))
        {
            if (_container.HasBinding(strategyType))
            {
                var strategy = (IMoveExecutionStrategy)_container.Resolve(strategyType);
                _cachedStrategies[mode] = strategy; // 快取
                return strategy;
            }
        }

        Debug.LogWarning($"策略 {mode} 未在此場景綁定，返回 null");
        return null;
    }
}
