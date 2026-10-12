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

    // === 轉場佇列化 ===
    private readonly Queue<GamePhaseState> _pendingTransitions = new Queue<GamePhaseState>();
    private bool _isTransitioning;
    private int _phaseVersion;
    private const int MAX_TRANSITIONS_PER_DRAIN = 12; // 安全閥，非死鎖判斷用的合法上限
    /// <summary>
    /// 每次 ApplyPhase 成功套用就 +1。任何跨 frame / 跨 await 恢復執行的協程，
    /// 應該在啟動時記住這個值，恢復執行後用 IsCurrent() 檢查，
    /// 確保自己沒有在已經被 OnExit 之後還去動 Model 或發訊號。
    /// </summary>
    public int PhaseVersion => _phaseVersion;
    public bool IsCurrent(int version) => version == _phaseVersion;
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
        _pendingTransitions.Clear();
        _currentPhase?.OnExit(this);
        _currentPhase = null;
        _currentPhaseState = default;
        _phaseVersion++;
    }

    public void ChangePhase(GamePhaseState newPhase)
    {
        _pendingTransitions.Enqueue(newPhase);

        if (_isTransitioning) return;
        _isTransitioning = true;

        try
        {
            int safetyCounter = 0;
            while (_pendingTransitions.Count > 0)
            {
                if (++safetyCounter > MAX_TRANSITIONS_PER_DRAIN)
                {
                    Debug.LogError($"[GamePhaseStateMachine] 疑似轉場迴圈！連續套用 {MAX_TRANSITIONS_PER_DRAIN} 次仍未停下，" +
                                    $"強制清空剩餘 {_pendingTransitions.Count} 筆請求，避免主執行緒卡死。請檢查是否有 Phase 互相觸發。");
                    _pendingTransitions.Clear();
                    break;
                }
                ApplyPhase(_pendingTransitions.Dequeue());
            }
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private void ApplyPhase(GamePhaseState newPhase)
    {
        GamePhaseState previousPhase = _currentPhaseState;

        _currentPhase?.OnExit(this);
        _currentPhase = _phases[newPhase];
        _currentPhaseState = newPhase;
        _phaseVersion++;
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
