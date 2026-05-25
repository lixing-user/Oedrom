using UnityEngine;
using System.Collections;
using Zenject;

// Application / Phase / MainFirstPhase.cs 
public class MainFirstPhase : IGamePhaseState
{
    private GamePhaseStateMachine _machine;
    private GamePhaseModel _model;
    private GamePhaseFacade _facade;

    private SignalBus _signalBus;

    private bool _isWaitingForPlayer = true;
    private bool _phaseSkipped = false;

    #region 介面實作
    public void OnEnter(GamePhaseStateMachine machine)
    {
        _machine = machine;
        _model = machine.GetModel();
        _facade = machine.GetFacade();
        _signalBus = machine.GetSignalBus();

        Debug.Log("[MainPhase1] === 進入主要階段1（卡牌放置） ===");

        _model.CanProgressToNextPhase = false;

        _signalBus.Subscribe<PieceSpawnedSignal>(OnCardPlaced);

        if (_facade.GetCurrentTeam() == Team.Black) // 假設 Black 是 AI
        {
            Debug.Log("[MainPhase1] AI回合，不需要打牌，自動跳過進入移動階段！");
            _model.CanProgressToNextPhase = true;
            _machine.ChangePhase(GamePhaseState.MovementPhase);
            return;
        }

        _machine.StartCoroutine(ExecutePhaseCoroutine());
    }

    public void OnUpdate(GamePhaseStateMachine machine)
    {
        if (_facade.IsPieceCurrentlySelected() && _model.CanSwitchToMovement)
        {
            Debug.Log("[MainPhase1] 偵測到棋子操作，切換到移動階段");
            SwitchToMovementPhase();
        }
    }

    public void OnExit(GamePhaseStateMachine machine)
    {
        Debug.Log("[MainPhase1] === 離開主要階段1 ===");
        Debug.Log($"  → 放置卡牌數量: {_model.PlacedCardCount}");
        Debug.Log($"  → 是否跳過: {_phaseSkipped}");

        _signalBus.Unsubscribe<PieceSpawnedSignal>(OnCardPlaced);
    }
    #endregion

    #region 核心邏輯
    private IEnumerator ExecutePhaseCoroutine()
    {
        // === 步驟 1：開啟輸入 ===
        _facade.EnableAllInput();

        yield return new WaitForSeconds(0.3f);

        Debug.Log("[MainPhase1] 等待玩家操作（不會自動切換）...");
    }

    private void SwitchToMovementPhase()
    {
        if (_model.HasPlacedCardThisTurn)
        {
            _facade.DisableCardInput();
            _model.CanSwitchToCardPlacement = false;
            Debug.Log("[MainPhase1] 已放置卡牌，鎖定卡牌輸入");
        }

        // === 步驟 4：標記完成 ===
        _model.CanProgressToNextPhase = true;

        // === 步驟 5：進入下一階段 ===
        _machine.ChangePhase(GamePhaseState.MovementPhase);
        Debug.Log("[MainPhase1] === 主要階段1結束，進入移動階段 ===");
    }
    #endregion

    #region 信號處理
    private void OnCardPlaced(PieceSpawnedSignal signal)
    {
        _model.PlacedCardCount++;
        _model.HasPlacedCardThisTurn = true;
        Debug.Log($"[MainPhase1] 已放置卡牌 #{_model.PlacedCardCount}");

        _facade.DisableCardInput();
        _model.CanSwitchToCardPlacement = false;
        Debug.Log("[MainPhase1] 已達到放置上限，立刻鎖定卡牌輸入");
    }
    #endregion
}
