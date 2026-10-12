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

    private const Team PlayerTeam = Team.White;
    private Team _currentTeam = PlayerTeam;
    private GamePhaseState _currentPhase = GamePhaseState.StandbyPhase;


    [Header("Phase Popup")]
    [Tooltip("只有「玩家回合」且屬於此清單的階段才會顯示階段提示。")]
    [SerializeField]
    private GamePhaseState[] _popupPhases =
   {
        GamePhaseState.MainPhase1,
        GamePhaseState.MovementPhase,
        GamePhaseState.ResolutionPhase
    };


    #region Unity Lifecycle
    private void Awake()
    {
        SubscribeToSignals();

        _view = this.GetComponent<GameFlowView>();
        if (_view == null)
        {
            Debug.LogError("[GameStatePresenter] 找不到 GameStateView！");
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

        _currentTeam = _turnManager.CurrentTurn;

        string turnText = _flowText.GetTurnText(_currentTeam);
        _view.UpdateFlowState(_currentTeam, turnText);

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
        //_view.ShowTransitionPopup(
        //    _flowText.GetText("game_start"),
        //    FlowTransitionType.TurnChange
        //);
        if (_view == null) return;

        _view.ShowTurnPopup(_flowText.GetText("game_start"));
    }

    private void OnTurnChanged(GlobalTurnChangedSignal signal)
    {
        //_currentTeam = signal.CurrentTurn;

        //string turnText = _flowText.GetTurnText(signal.CurrentTurn);
        //_view.UpdateFlowState(signal.CurrentTurn, turnText);

        //string popupText = signal.CurrentTurn == Team.White
        //    ? _flowText.GetText("your_turn")
        //    : _flowText.GetText("enemy_turn");

        //_view.ShowTransitionPopup(popupText, FlowTransitionType.TurnChange);
        if (_view == null) return;

        _currentTeam = signal.CurrentTurn;

        _view.UpdateFlowState(_currentTeam, _flowText.GetTurnText(_currentTeam));
        _view.ShowTurnPopup(_flowText.GetTurnPopupText(_currentTeam));
    }

    private void OnPhaseChanged(PhaseChangedSignal signal)
    {
        if (_view == null) return;

        _currentPhase = signal.CurrentPhase;

        string phaseText = _flowText.GetPhaseText(_currentPhase);
        _view.UpdatePhaseState(_currentPhase, phaseText);

        if (ShowPhasePopup(_currentPhase))
        {
            // _view.ShowTransitionPopup(phaseText, FlowTransitionType.PhaseChange);
            _view.ShowPhasePopup(phaseText);
        }
    }
    #endregion

    #region Helper Methods
    private bool ShowPhasePopup(GamePhaseState phase)
    {
        if (_currentTeam != PlayerTeam) return false;

        return System.Array.IndexOf(_popupPhases, phase) >= 0;
    }
    #endregion
}
