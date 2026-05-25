using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IGamePhaseState
{
    void OnEnter(GamePhaseStateMachine machine);
    void OnUpdate(GamePhaseStateMachine machine);
    void OnExit(GamePhaseStateMachine machine);
}


// 第三層 ： 嵌套在 PlayerTurnState 內部
public class GamePhaseStateMachine : MonoBehaviour
{
    private Dictionary<GamePhaseState, IGamePhaseState> _phases;
    private IGamePhaseState _currentPhase;
    private GamePhaseState _currentPhaseState;
    private GamePhaseModel _model;
    private GamePhaseFacade _facade;
    private SignalBus _signalBus;
    [Inject] private readonly DiContainer _container;

    public event Action OnAllPhasesCompleted;

    [Inject]
    public void Construct(
        GamePhaseFacade facade,
        GamePhaseModel model,
        SignalBus signalBus)
    {
        _facade = facade;
        _model = model; 
        _signalBus = signalBus;
        InitializePhases();
    }


    private void InitializePhases()
    {
        _phases = new Dictionary<GamePhaseState, IGamePhaseState>
        {
            { GamePhaseState.StandbyPhase, _container.Instantiate<StandbyPhase>() },
            { GamePhaseState.MainPhase1, _container.Instantiate < MainFirstPhase >() },
            { GamePhaseState.MovementPhase, _container.Instantiate < MovementPhase >() },
            { GamePhaseState.MainPhase2, _container.Instantiate<MainSecPhase>() },
            { GamePhaseState.ResolutionPhase, _container.Instantiate < ResolutionPhase >() },
            { GamePhaseState.EndPhase, _container.Instantiate < EndPhase >() }
        };
    }

    public void StartPhases(GamePhaseState startPhase)
    {
        ChangePhase(startPhase);
    }

    public void StopPhases()
    {
        _currentPhase?.OnExit(this);
        _currentPhase = null;
    }

    public void ChangePhase(GamePhaseState newPhase)
    {
        GamePhaseState previousPhase = _currentPhaseState;

        _currentPhase?.OnExit(this);
        _currentPhase = _phases[newPhase];
        _currentPhaseState = newPhase;
        _currentPhase.OnEnter(this);

        _signalBus.Fire(new PhaseChangedSignal(newPhase, previousPhase));
    }

    public void TriggerAllPhasesCompleted()
    {
        Debug.Log("[GamePhaseStateMachine] 所有階段已完成，觸發事件");
        OnAllPhasesCompleted?.Invoke();
    }

    void Update()
    {
        _currentPhase?.OnUpdate(this);
    }

    public GamePhaseModel GetModel() => _model;
    public GamePhaseFacade GetFacade() => _facade;
    public GamePhaseState GetCurrentPhaseState() => _currentPhaseState;
    public SignalBus GetSignalBus() => _signalBus;
}
