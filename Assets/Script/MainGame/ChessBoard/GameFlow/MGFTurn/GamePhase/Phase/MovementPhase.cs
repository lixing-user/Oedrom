using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#region New Version
// Application / Phase / MovementPhase.cs
public class MovementPhase : IGamePhaseState
{
    [Inject] private List<IPhaseInputProvider> _inputProviders;

    private GamePhaseStateMachine _machine;
    private GamePhaseModel _model;
    private GamePhaseFacade _facade;
    private SignalBus _signalBus;

    private bool _isWaitingForVisual = false;

    #region 介面實作
    public void OnEnter(GamePhaseStateMachine machine)
    {
        _machine = machine;
        _model = machine.GetModel();
        _facade = machine.GetFacade();
        _signalBus = machine.GetSignalBus();

        Debug.Log("[MovementPhase] === 進入移動階段 ===");

        _model.CanProgressToNextPhase = false;
        _isWaitingForVisual = false;

        _signalBus.Subscribe<PreviewMoveSignal>(OnPreviewMove);
        _signalBus.Subscribe<PieceMovedSignal>(OnPieceMoved);
        _signalBus.Subscribe<MovesConfirmedSignal>(OnMovesConfirmed);
        _signalBus.Subscribe<EnemyVisualMoveCompletedSignal>(OnVisualMoveCompleted);

        ExecutePhaseSequence();
    }
    public void OnUpdate(GamePhaseStateMachine machine)
    {
        if (_facade.IsCardCurrentlySelected() && !_facade.IsPieceCurrentlySelected() && _model.CanSwitchToCardPlacement)
        {
            Debug.Log("[MovementPhase] 偵測到卡牌操作，切換回卡牌階段");
            SwitchToCardPhase();
        }
    }
    public void OnExit(GamePhaseStateMachine machine)
    {
        Debug.Log("[MovementPhase] === 離開移動階段 ===");
        Debug.Log($"  → 是否移動: {_model.HasMovedThisTurn}");
        Debug.Log($"  → 最終位置: {_model.LastMovedPiecePosition}");

        _signalBus.Unsubscribe<PreviewMoveSignal>(OnPreviewMove);
        _signalBus.Unsubscribe<PieceMovedSignal>(OnPieceMoved);
        _signalBus.Unsubscribe<MovesConfirmedSignal>(OnMovesConfirmed);
        _signalBus.Unsubscribe<EnemyVisualMoveCompletedSignal>(OnVisualMoveCompleted);

        _facade.DisableAllInput();
    }
    #endregion

    #region 核心邏輯
    private async void ExecutePhaseSequence()
    {
        Debug.Log("[MovementPhase] 開始執行階段流程...");

        // 1. 決定誰是目前的控制者 (Provider)
        var currentProvider = _inputProviders.Find(p => p.Team == _facade.GetCurrentTeam());

        // 2. 獲取移動指令 (如果是玩家，會在這裡卡住直到他按「結束回合」)
        List<PendingPieceMove> moves = await currentProvider.GetMovementCommandsAsync();

        // 3. 【關鍵】由 Phase 統一發射執行訊號
        if (moves != null && moves.Count > 0)
        {
            // 【新增細節】如果是 AI，也要把指令寫入 PendingMoves，確保資料對稱性
            if (_facade.GetCurrentTeam() == Team.Black)
            {
                _model.ClearPendingMoves();
                foreach (var m in moves) _model.AddPendingMove(m.PieceId, m.From, m.To);
            }

            // 準備進入視覺等待鎖 (如果是 AI 回合)
            if (_facade.GetCurrentTeam() == Team.Black) _isWaitingForVisual = true;

            Debug.Log($"[MovementPhase] 收到 {moves.Count} 個移動，發射 BatchConfirmMovesSignal");
            _signalBus.Fire(new BatchConfirmMovesSignal(moves));
        }
        else
        {
            Debug.Log("[MovementPhase] 無移動指令，直接進入下一階段");
            SwitchToNextPhase();
        }
    }

    private void SwitchToCardPhase()
    {
        _model.CanProgressToNextPhase = true;

        _machine.ChangePhase(GamePhaseState.MainPhase1);
        Debug.Log("[MovementPhase] === 返回卡牌階段 ===");
    }

    private void SwitchToNextPhase()
    {
        // === 步驟 4：標記完成 ===
        _model.CanProgressToNextPhase = true;

        // === 步驟 5：進入下一階段 ===
        _machine.ChangePhase(GamePhaseState.MainPhase2);

        Debug.Log("[MovementPhase] === 移動階段結束，進入主要階段2 ===");
    }
    #endregion

    #region 信號處理
    public void OnPreviewMove(PreviewMoveSignal signal)
    {
        var fromV2 = new Vector2Int(signal.From.X, signal.From.Y);
        var toV2 = new Vector2Int(signal.To.X, signal.To.Y);
        _model.AddPendingMove(signal.PieceId, fromV2, toV2);
        Debug.Log($"[MovementPhase] 記錄預覽: {signal.PieceId}");
    }

    private void OnPieceMoved(PieceMovedSignal signal)
    {
        // 記錄移動資訊到 Model（用於 EndPhase 計算補給）
        Vector2Int fromPos = new Vector2Int(
               signal.FromPosition.X,
               signal.FromPosition.Y
           );

        Vector2Int toPos = new Vector2Int(
            signal.ToPosition.X,
            signal.ToPosition.Y
        );

        _model.AddMovementRecord(signal.PieceId, fromPos, toPos);
        _model.LastMovedPiecePosition = toPos;
        _model.LastMovedPieceId = signal.PieceId;

        Debug.Log($"[MovementPhase] 已記錄移動: {signal.PieceId} 從 {fromPos} 到 {toPos}");
        _model.HasMovedThisTurn = true;
    }

    private void OnMovesConfirmed()
    {
        Debug.Log("[MovementPhase] 步驟4: 批量確認完成");
        if (_facade.GetCurrentTeam() == Team.White)
            SwitchToNextPhase();
        else
        {
            Debug.Log("[MovementPhase] 邏輯已確認，等待 AI 視覺動畫結束...");
            // 如果視覺已經先跑完了（極少見），就直接切換
            if (!_isWaitingForVisual) SwitchToNextPhase();
        }
    }

    private void OnVisualMoveCompleted(EnemyVisualMoveCompletedSignal signal)
    {
        _isWaitingForVisual = false;
        Debug.Log("[MovementPhase] AI 視覺動畫完成，準備進入主要階段2");

        // 只有在邏輯確認訊號也收到的情況下才切換 (確保邏輯在前)
        SwitchToNextPhase();
    }
    #endregion
}
#endregion

