using System;
using UnityEngine;
using Zenject;

public class PhaseControlPresenter : MonoBehaviour
{
    [Inject] private readonly SignalBus _signalBus;

    private IPhaseControlView _view;

    #region Lifecycle
    void Awake()
    {
        _view = GetComponent<IPhaseControlView>();
        _view.OnEndPhaseRequested += HandleEndPhaseRequested;
        _signalBus.Subscribe<PhaseChangedSignal>(OnPhaseChanged);
    }
    void OnDestroy()
    {
        _view.OnEndPhaseRequested -= HandleEndPhaseRequested;
        _signalBus.Unsubscribe<PhaseChangedSignal>(OnPhaseChanged);
    }
    #endregion

    #region Event Handlers
    private void OnPhaseChanged(PhaseChangedSignal signal)
    {
        UpdateUIForPhase(signal.CurrentPhase);
    }


    private void HandleEndPhaseRequested()
    {
        Debug.Log("[PhaseControlPresenter] 玩家請求結束階段");
        _signalBus.Fire(new PhaseEndRequestedSignal("Button"));
    }


    private void UpdateUIForPhase(GamePhaseState phase)
    {
        switch (phase)
        {
            case GamePhaseState.MainPhase1:
                _view.Show();
                _view.ShowPhaseHint("放置卡牌或選擇棋子");
                break;

            case GamePhaseState.MovementPhase:
                _view.Show();
                _view.ShowPhaseHint("移動棋子或結束階段");
                break;

            default:
                _view.Hide();
                break;
        }
    }
    #endregion
}
