using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


public interface IGameFlowState
{
    void OnEnter(GameFlowStateMachine machine);
    void OnExit(GameFlowStateMachine machine);
}


// 最上層 ： 負責回合切換和跨場景的全域流程 
public class GameFlowStateMachine : IInitializable, IDisposable
{
    private Dictionary<GameFlow, IGameFlowState> _states;
    private IGameFlowState _currentState;

    private GameFlow _currentFlow;
    private GameFlow _previousFlow;
    public GameFlow CurrentFlow => _currentFlow;

    [Inject] private IFlowFacade _flowFacade;

    [Inject] private readonly ITurnManager _turnManager;
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly DiContainer _container;
    [Inject] private readonly IGameFlowStateFactory _stateFactory;

    /// 回合數據
    private int _turnNumber = 0;
    private Team _previousTeam = Team.White;


    #region Lifecycle
    public void Initialize()
    {
        InitializeStates();

        _turnManager.StartGame();
        //_signalBus.Fire(new GameStartedSignal(
        //Team.White,
        //UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        //));

        ChangeState(GameFlow.Setup);
    }

    private void InitializeStates()
    {
        _states = new Dictionary<GameFlow, IGameFlowState>
        {
            { GameFlow.Setup, _stateFactory.CreateSetupState(this, _container) },
            { GameFlow.PlayerTurn, _stateFactory.CreatePlayerTurnState(this, _container) },
            { GameFlow.EnemyTurn, _stateFactory.CreateEnemyTurnState(this, _container) },
            { GameFlow.Resolution, _stateFactory.CreateResolutionState(this, _container) },
            { GameFlow.GameEnd, _stateFactory.CreateEndGameState(this, _container) }
        };
    }

    public void Dispose()
    {
        _currentState?.OnExit(this);
        _currentState = null;
    }
    #endregion 


    public void ChangeState(GameFlow newFlow)
    {
        // 退出當前狀態
        _currentState?.OnExit(this);

        // 記錄前一個流程狀態
        _previousFlow = _currentFlow;
        _currentFlow = newFlow;

        // 切換到新狀態
        _currentState = _states[newFlow];

        // 確定新的回合歸屬
        Team newTeam = DetermineTeam(newFlow);

        // 更新 TurnManager（觸發 TurnManager 的 OnTurnChanged Event）
        if (newTeam != _turnManager.CurrentTurn || _previousFlow == GameFlow.Setup)
        {
            ((TurnManager)_turnManager).SetTurn(newTeam);

            _signalBus.Fire(new TurnChangedSignal(newTeam, _previousTeam));

            _previousTeam = newTeam;
        }

        // 更新 TurnManager 的回合狀態（PlayerTurn 或 EnemyTurn）
        if ((newFlow == GameFlow.PlayerTurn || newFlow == GameFlow.EnemyTurn) &&
                 (_previousFlow != newFlow))
        {
            _turnNumber++;

            // 發送全域級 Signal（用於跨場景統計、存檔等系統）
            _signalBus.Fire(new GlobalTurnChangedSignal(
                newTeam,
                _turnNumber,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            ));

            Debug.Log($"[GameFlow] 回合 {_turnNumber} - {newTeam} 開始");
        }

        _signalBus.Fire(new FlowChangedSignal(_currentFlow, _previousFlow));

        // 進入新狀態
        _currentState.OnEnter(this);
    }


    /// <summary>
    /// 根據 GameFlow 決定對應的回合歸屬
    /// </summary>
    private Team DetermineTeam(GameFlow flow)
    {
        return flow switch
        {
            GameFlow.PlayerTurn => Team.White,
            GameFlow.EnemyTurn => Team.Black,
            GameFlow.Resolution => _turnManager.CurrentTurn, // 保持當前回合
            _ => Team.White
        };
    }


    // 提供給子狀態訪問的介面
    public ITurnManager GetTurnManager() => _turnManager;
    public SignalBus GetSignalBus() => _signalBus;
    public IFlowFacade GetFlowFacade() => _flowFacade;
    public int GetCurrentTurnNumber() => _turnNumber;
    public GameFlow GetCurrentFlow() => _currentFlow;
    public GameFlow GetPreviousFlow() => _previousFlow;
}
