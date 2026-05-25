using UnityEngine;
using Zenject;


public class GameFlowPresenterS : MonoBehaviour
{
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly FlowTextLocalization _flowText;
    [Inject] private ITurnManager _turnManager;

    private GameFlowViewS _view;

    private Team _currentTeam = Team.White;
    private GameFlow _currentFlow = GameFlow.Setup;


    #region Unity Lifecycle
    private void Awake()
    {
        SubscribeToSignals();

        _view = this.GetComponent<GameFlowViewS>();
        if (_view == null)
        {
            Debug.LogError("[GameStatePresenter] §ä¤£¨ì GameStateView¡I");
        }
    }

    private void Start()
    {
        InitializeUIState();
    }

    private void OnDestroy()
    {
        UnsubscribeFromSignals();
    }

    private void InitializeUIState()
    {
        if (_view == null) return;

        string turnText = _flowText.GetTurnText(_turnManager.CurrentTurn);
        string flowText = _flowText.GetFlowText(_currentFlow);
        _view.UpdateTurnState(_turnManager.CurrentTurn, turnText);
        _view.UpdateFlowState(_currentFlow, flowText);
    }
    #endregion

    #region Signal Subscription
    private void SubscribeToSignals()
    {
        _signalBus.Subscribe<TurnChangedSignal>(OnTurnChanged);
        _signalBus.Subscribe<FlowChangedSignal>(OnFlowChanged);
        _signalBus.Subscribe<GameStartedSignal> (OnGameStarted);
        //_signalBus.Subscribe<GameEndedSignal>(OnBattleEnded);
    }
    private void UnsubscribeFromSignals()
    {
        _signalBus.Unsubscribe<TurnChangedSignal>(OnTurnChanged);
        _signalBus.Unsubscribe<FlowChangedSignal>(OnFlowChanged);
        _signalBus.Unsubscribe<GameStartedSignal> (OnGameStarted);
        //_signalBus.Unsubscribe<GameEndedSignal> (OnBattleEnded);
    }
    #endregion

    #region Signal Handlers
    private void OnGameStarted(GameStartedSignal signal)
    {
        _view.ShowTransitionPopup(
            _flowText.GetText("game_start"),
            FlowTransitionType.TurnChange
        );
    }

    private void OnTurnChanged(TurnChangedSignal signal)
    {
        _currentTeam = signal.CurrentTurn;

        string turnText = _flowText.GetTurnText(signal.CurrentTurn);
        _view.UpdateTurnState(signal.CurrentTurn, turnText);

        string popupText = signal.CurrentTurn == Team.White
            ? _flowText.GetText("your_turn")
            : _flowText.GetText("enemy_turn");

        _view.ShowTransitionPopup(popupText, FlowTransitionType.TurnChange);
    }

    private void OnFlowChanged(FlowChangedSignal signal)
    {
        _currentFlow = signal.CurrentFlow;
         
        string flowText = _flowText.GetFlowText(signal.CurrentFlow);
        _view.UpdateFlowState(signal.CurrentFlow, flowText);

        if (ShowFlowPopup(signal.CurrentFlow))
        {
            _view.ShowTransitionPopup(flowText, FlowTransitionType.TurnChange);
        }
    }

    //private void OnBattleEnded(GameEndedSignal signal)
    //{

    //}
    #endregion

    #region Helper Methods

    private bool ShowFlowPopup(GameFlow phase)
    {
        return phase switch
        {
            GameFlow.Setup => true,
            GameFlow.PlayerTurn => true,
            GameFlow.EnemyTurn => true,
            GameFlow.Resolution => true,
            GameFlow.GameEnd => true,
            _ => false
        };
    }
    #endregion
}
