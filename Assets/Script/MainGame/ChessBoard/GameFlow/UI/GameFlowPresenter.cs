using UnityEngine;
using Zenject;


public enum FlowTransitionType
{
    TurnChange,
    PhaseChange
}


public class GameFlowPresenter : MonoBehaviour
{
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly FlowTextLocalization _flowText;
    [Inject] private ITurnManager _turnManager;

    private GameFlowView _view;

    private Team _currentTeam = Team.White;
    private GamePhaseState _currentPhase = GamePhaseState.StandbyPhase;


    #region Unity Lifecycle
    private void Awake()
    {
        SubscribeToSignals();

        _view = this.GetComponent<GameFlowView>();
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
        _view.UpdateFlowState(_turnManager.CurrentTurn, turnText);

        string phaseText = _flowText.GetPhaseText(_currentPhase);
        _view.UpdatePhaseState(_currentPhase, phaseText);
    }
    #endregion

    #region Signal Subscription
    private void SubscribeToSignals()
    {
        _signalBus.Subscribe<GlobalTurnChangedSignal>(OnTurnChanged);
        _signalBus.Subscribe<PhaseChangedSignal>(OnPhaseChanged);
        _signalBus.Subscribe<GameStartedSignal> (OnGameStarted);
    }
    private void UnsubscribeFromSignals()
    {
        _signalBus.Unsubscribe<GlobalTurnChangedSignal>(OnTurnChanged);
        _signalBus.Unsubscribe<PhaseChangedSignal>(OnPhaseChanged);
        _signalBus.Unsubscribe<GameStartedSignal> (OnGameStarted);
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

    private void OnTurnChanged(GlobalTurnChangedSignal signal)
    {
        _currentTeam = signal.CurrentTurn;

        string turnText = _flowText.GetTurnText(signal.CurrentTurn);
        _view.UpdateFlowState(signal.CurrentTurn, turnText);

        string popupText = signal.CurrentTurn == Team.White
            ? _flowText.GetText("your_turn")
            : _flowText.GetText("enemy_turn");

        _view.ShowTransitionPopup(popupText, FlowTransitionType.TurnChange);
    }

    private void OnPhaseChanged(PhaseChangedSignal signal)
    {
        _currentPhase = signal.CurrentPhase;

        string phaseText = _flowText.GetPhaseText(signal.CurrentPhase);
        _view.UpdatePhaseState(signal.CurrentPhase, phaseText);

        if (ShowPhasePopup(signal.CurrentPhase))
        {
            _view.ShowTransitionPopup(phaseText, FlowTransitionType.PhaseChange);
        }
    }
    #endregion

    #region Helper Methods

    private bool ShowPhasePopup(GamePhaseState phase)
    {
        return phase switch
        {
            GamePhaseState.MainPhase1 => true,
            GamePhaseState.MovementPhase => true,
            GamePhaseState.ResolutionPhase => true,
            _ => false
        };
    }
    #endregion
}
