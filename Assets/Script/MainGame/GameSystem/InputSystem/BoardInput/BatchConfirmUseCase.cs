using System;
using UnityEngine;
using Zenject;

// Application/ use case
public class BatchConfirmMovesHandler : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly GamePhaseModel _model; 
    private readonly GameBoard<TerrainTile> _board; 
    private readonly ITerrainRepository _terrainRepository;
    private readonly IMoveHandler _moveHandler;

    public BatchConfirmMovesHandler(
        SignalBus signalBus,
        IMoveHandler moveHandler,
        GamePhaseModel model,
        GameBoard<TerrainTile> board,
        ITerrainRepository terrainRepository)
    {
        _signalBus = signalBus;
        _moveHandler = moveHandler;
        _model = model;
        _board = board;
        _terrainRepository = terrainRepository;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<BatchConfirmMovesSignal>(Handle);
    }

    public void Dispose()
    {
        _signalBus.Unsubscribe<BatchConfirmMovesSignal>(Handle);
    }

    private void Handle(BatchConfirmMovesSignal signal)
    {
        Debug.Log($"[BatchConfirmMovesHandler] 執行 {signal.PendingMoves.Count} 個移動");

        foreach (var pending in signal.PendingMoves)  // 用快照數據
        {
            var pieceId = pending.PieceId;
            var to = new PiecePosition(pending.To.x, pending.To.y);
            var piece = _board.GetPieceById(pieceId);

            _model.LastMovedPieceId = pieceId;

            // --- 1. 優先判定戰鬥 ---
            var targetPiece = _board.GetPieceAt(to);
            if (targetPiece != null && targetPiece.IsEnemyOf(piece))
            {
                // 戰鬥：只做視覺移動，不移動數據
                HandleBattlePreview(piece, targetPiece, pending);
                continue; // 判定結束
            }

            // --- 2. 執行普通移動 ---
            if (piece.Team == Team.Black)
            {
                // AI 的行動直接強制移動，徹底繞過 MoveValidatorT 的地形與移動範圍驗證
                _moveHandler.ForceMove(pieceId, to);
                Debug.Log($"[BatchConfirmMovesHandler] AI 強制移動 {pieceId} 至 {to}，無視地形限制");
            }
            else
            {
                // 玩家的行動使用一般 MoveTo，會觸發 _validator.IsValidMove 檢查地形
                _moveHandler.MoveTo(pieceId, to);
            }

            // --- 3. 判定地標 / 商店 ---
            var landmark = _terrainRepository.GetLandmarkAt(to);
            if (landmark != LandmarkType.None && landmark == LandmarkType.Town)
            {
                _model.CurrentResolutionType = ResolutionType.Shop;
                _model.TriggerPosition = pending.To;
                continue; // 判定結束
            }

            // --- 4. 隨機事件---
            //float triggerProbability = 0.3f; // 這裡建議未來從 Config 讀取

            if (UnityEngine.Random.value <= 1.0f)
            {
                Debug.Log($"[BatchConfirmMovesHandler] 隨機事件觸發成功！位置: {to}");

                _model.IsRandomEventTriggered = true;
                _model.CurrentResolutionType = ResolutionType.RandomEvent;

                // 記錄觸發位置，以便後續可能的效果需要（例如在該位置生成東西）
                _model.TriggerPosition = pending.To;
                continue;// 判定結束
            }

            // --- 5. 什麼都沒發生 (預設狀態) ---
            _model.CurrentResolutionType = ResolutionType.None;
        }

        _model.ClearPendingMoves();
        _signalBus.Fire(new MovesConfirmedSignal());
    }


    private void HandleBattlePreview(
       ChessPiece attacker,
       ChessPiece defender,
       PendingPieceMove pending)
    {
        Debug.Log($"[BatchConfirmMovesHandler] 檢測到戰鬥：{attacker.Id} vs {defender.Id}");

        // --- 判定戰鬥類型 ---
        BattleType resolvedBattleType = BattleType.Normal;

        if (defender.Team == Team.Black && defender.Type == ChessPieceType.King)
        {
            resolvedBattleType = BattleType.Final;
            Debug.Log($"[BatchConfirmMovesHandler] 偵測到攻擊國王，設定為 FinalBattle！");
        }

        _model.PendingBattle = new BattleSessionData
        {
            BattleId = Guid.NewGuid().ToString(),
            BattleType = resolvedBattleType,
            AttackerId = attacker.Id,
            DefenderId = defender.Id,
            AttackerPos = attacker.CurrentPosition,
            DefenderPos = defender.CurrentPosition,
            AttackerType = attacker.Type,    
            DefenderType = defender.Type,    
            AttackerTeam = attacker.Team,     
            DefenderTeam = defender.Team,     
        };

        _model.HasMovedThisTurn = true;
        _model.LastMovedPiecePosition = new Vector2Int(
            defender.CurrentPosition.X,
            defender.CurrentPosition.Y
        );

        // 觸發視覺移動 + 錯開站位
        var battleSignal = BattleStartSignal.Create(
            attacker,
            defender,
            attacker.CurrentPosition,
            defender.CurrentPosition
        );
        _signalBus.Fire(battleSignal);

        // 標記戰鬥觸發
        _model.IsBattleTriggered = true;

        Debug.Log("[BatchConfirmMovesHandler] 戰鬥數據已凍結，等待結算");
    }
}