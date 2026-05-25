using UnityEngine;
using Zenject;

public interface IMoveModeService
{
    MoveExecutionMode CurrentMode { get; }
    void SetMode(MoveExecutionMode mode);
    IMoveExecutionStrategy GetCurrentStrategy();
}

public class MoveModeService : IMoveModeService
{
    private readonly IMoveExecutionStrategyFactory _strategyFactory;
    private readonly SignalBus _signalBus;

    private MoveExecutionMode _currentMode;
    private IMoveExecutionStrategy _currentStrategy;

    public MoveExecutionMode CurrentMode => _currentMode;

    public MoveModeService(
       IMoveExecutionStrategyFactory strategyFactory,
       SignalBus signalBus,
       MoveExecutionMode defaultMode = MoveExecutionMode.Direct)
    {
        _strategyFactory = strategyFactory;
        _signalBus = signalBus;

        SetMode(defaultMode);
    }


    public void SetMode(MoveExecutionMode mode)
    {
        var strategy = _strategyFactory.GetStrategy(mode);

        if (strategy == null)
        {
            Debug.LogError($"[MoveModeService] 家Α {mode} ゼ竕﹚玂讽玡家Α {_currentMode}");
            return;
        }

        var previousMode = _currentMode;
        _currentMode = mode;
        _currentStrategy = strategy;

        // 祇癳家Αち传獺腹
        _signalBus.Fire(new MoveModeChangedSignal(mode, previousMode));

        Debug.Log($"[MoveModeService] 簿笆家Αち传: {previousMode} △ {mode}");
    }

    public IMoveExecutionStrategy GetCurrentStrategy()
    {
        return _currentStrategy;
    }
}


// 簿笆家Αち传獺腹(耎籔UI╰参羛笆)
public class MoveModeChangedSignal
{
    public MoveExecutionMode NewMode { get; }
    public MoveExecutionMode PreviousMode { get; }

    public MoveModeChangedSignal(MoveExecutionMode newMode, MoveExecutionMode previousMode)
    {
        NewMode = newMode;
        PreviousMode = previousMode;
    }
}