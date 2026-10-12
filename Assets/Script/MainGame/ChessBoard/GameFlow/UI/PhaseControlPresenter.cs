using System;
using UnityEngine;
using Zenject;

public class PhaseControlPresenter : MonoBehaviour
{
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly GamePhaseFacade _facade;

    private GamePhaseState _currentPhase;
    private IPhaseControlView _view;

    #region Lifecycle
    void Awake()
    {
        _view = GetComponent<IPhaseControlView>();
        _view.OnEndPhaseRequested += HandleEndPhaseRequested;
        _signalBus.Subscribe<PhaseChangedSignal>(OnPhaseChanged);
        _signalBus.Subscribe<PreviewMoveSignal>(OnPiecePreviewed);
    }
    void OnDestroy()
    {
        _view.OnEndPhaseRequested -= HandleEndPhaseRequested;
        _signalBus.Unsubscribe<PhaseChangedSignal>(OnPhaseChanged);
        _signalBus.Unsubscribe<PreviewMoveSignal>(OnPiecePreviewed);
    }
    #endregion

    #region Event Handlers
    private void OnPhaseChanged(PhaseChangedSignal signal)
    {
        _currentPhase = signal.CurrentPhase;

        if (_facade.GetCurrentTeam() != Team.White)
        {
            _view.Hide();
            return;
        }

        UpdateUIForPhase(signal.CurrentPhase);
    }


    private void OnPiecePreviewed(PreviewMoveSignal signal)
    {
        // 確保在移動階段，玩家一拉好預覽線，就閃爍按鈕提示他可以結束階段(或確認行動)
        if (_currentPhase == GamePhaseState.MovementPhase)
        {
            Debug.Log($"[PhaseControlPresenter] 偵測到棋子預覽移動: {signal.PieceId}，播放按鈕特效");
            _view.PlayHighlightEffect();
        }
    }


    private void HandleEndPhaseRequested()
    {
        Debug.Log("[PhaseControlPresenter] 玩家請求結束階段");

        _view.AnimateHideEndButton();

        _signalBus.Fire(new PhaseEndRequestedSignal("Button"));
    }


    private void UpdateUIForPhase(GamePhaseState phase)
    {
        switch (phase)
        {
            case GamePhaseState.MainPhase1:
                _view.Show();
                _view.AnimateHideEndButton();
                _view.ShowPhaseHint("放置卡牌或選擇棋子");
                break;

            case GamePhaseState.MovementPhase:
                _view.Show();
                _view.AnimateShowEndButton();
                _view.ShowPhaseHint("移動棋子或結束階段");
                break;

            default:
                _view.Hide();
                break;
        }
    }
    #endregion
}
