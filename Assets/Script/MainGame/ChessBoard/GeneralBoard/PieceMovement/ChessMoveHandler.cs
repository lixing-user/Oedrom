using System;
using UnityEngine;
using Zenject;


public interface IMoveHandler
{
    bool MoveTo(PieceID pieceId, PiecePosition to);
    bool SwapPieces(PiecePosition pos1, PiecePosition pos2);
    bool RestoreFromStandby(PieceID pieceId, PiecePosition to);

    bool ForceMove(PieceID pieceId, PiecePosition to);
    bool ForceCapture(PiecePosition position, ChessPiece capturer = null);
}


// Application 層：處理棋子移動邏輯（第四步）
public class ChessMoveHandler : IMoveHandler
{
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly IDomainEventDispatcher _eventDispatcher;

    [Inject] private readonly IGameBoard _board;
    [InjectOptional] private readonly IStandbyZone _standbyZone;

    [Inject] private readonly IMoveValidateService _validator;


    public bool MoveTo(PieceID pieceId, PiecePosition to)
    {
        if (!_validator.IsValidMove(pieceId, to))
            return false;

        var piece = _board.GetPieceById(pieceId);
        var from = piece.CurrentPosition;

        if (piece.State == PieceState.InStandby || piece.CurrentPosition == null)
        {
            Debug.Log($"[MoveHandler] 偵測到棋子 {piece.Type} 來自待命區，自動轉交 RestoreFromStandby 處理");
            return RestoreFromStandby(pieceId, to);
        }

        Debug.Log($"[MoveHandler] 移動請求: {piece.Type}({piece.Team}) from {from} to {to}");

        var targetPiece = _board.GetPieceAt(to);

        if (targetPiece != null)
        {
            Debug.Log($"[MoveHandler] 目標位置有棋子: {targetPiece.Type}({targetPiece.Team})");
            Debug.Log($"[MoveHandler] 是否為敵方: {piece.IsEnemyOf(targetPiece)}");
        }
        else
        {
            Debug.Log($"[MoveHandler] 目標位置為空");
        }

        if (targetPiece != null && piece.IsEnemyOf(targetPiece))
        {
            Debug.Log($"[MoveHandler] 檢測到敵方棋子 {targetPiece.Type}，執行吃子");

            // Step 1: 先吃掉敵方棋子
            if (!_board.CapturePiece(to, piece))
            {
                Debug.LogError($"[MoveHandler] 吃子失敗: {to}");
                return false;
            }

            // Step 2: 移動攻擊方到目標位置
            if (!_board.MovePiece(from, to))
            {
                Debug.LogError($"[MoveHandler] 移動失敗: {from} → {to}");
                return false;
            }

            Debug.Log($"[MoveHandler] 吃子完成: {piece.Type} 佔領 {to}");
        }
        else
        {
            if (!_board.MovePiece(from, to))   // 普通移動（目標位置為空或友軍）
            {
                Debug.LogWarning($"[MoveHandler] 普通移動失敗");
                return false;
            }
        }

        _eventDispatcher.Dispatch(_board.DomainEvents);
        _board.ClearEvents();

        return true;
    }


    public bool SwapPieces(PiecePosition pos1, PiecePosition pos2)
    {
        var piece1 = _board.GetPieceAt(pos1);
        var piece2 = _board.GetPieceAt(pos2);

        if (piece1 == null || piece2 == null)
        {
            Debug.LogWarning($"[MoveHandler] 交換失敗：位置無棋子");
            return false;
        }

        if (piece1.IsEnemyOf(piece2))
        {
            Debug.LogWarning($"[MoveHandler] 交換失敗：不能交換敵方棋子");
            return false;
        }

        if (!_board.SwapPieces(pos1, pos2))
            return false;

        _eventDispatcher.Dispatch(_board.DomainEvents);
        _board.ClearEvents();

        Debug.Log($"[MoveHandler] 交換成功：{pos1} <swap> {pos2}");
        return true;
    }

    public bool RestoreFromStandby(PieceID pieceId, PiecePosition to)
    {
        if (_standbyZone == null)
        {
            Debug.LogWarning("[MoveHandler] 當前場景不支持待命區功能");
            return false;
        }

        if (!_standbyZone.IsPieceInStandby(pieceId))
        {
            Debug.LogWarning($"[MoveHandler] 棋子 {pieceId} 不在待命區");
            return false;
        }

        if (_board.GetPieceAt(to) != null)
        {
            Debug.LogWarning($"[MoveHandler] 目標位置 {to} 已有棋子");
            return false;
        }

        // Step 1: 執行原有的恢復邏輯
        bool success = _board.RestorePiece(pieceId, to);

        if (success)
        {
            // Step 2: 新增角落/底線放置的轉向判斷
            var piece = _board.GetPieceById(pieceId);
            if (piece != null)
            {
                int maxY = _board.Dimensions.BoardSizeY - 1;

                if (piece.Team == Team.White && to.Y == maxY)
                {
                    piece.ReverseDirection();
                }
                else if (piece.Team == Team.Black && to.Y == 0)
                {
                    piece.ReverseDirection();
                }
            }

            // Step 3: 派發事件 (此時派發的事件會反映最新的 IsReversed 狀態)
            _eventDispatcher.Dispatch(_board.DomainEvents);
            _board.ClearEvents();
            Debug.Log($"[MoveHandler] 從待命區恢復成功: {pieceId} → {to}");
        }
        else
        {
            Debug.LogError($"[MoveHandler] 從待命區恢復失敗");
        }

        return success;
    }


    public bool ForceMove(PieceID pieceId, PiecePosition to)
    {
        var piece = _board.GetPieceById(pieceId);
        if (piece == null) return false;

        var from = piece.CurrentPosition;
        Debug.Log($"[MoveHandler] 攻擊方實體自認的 CurrentPosition: {piece.CurrentPosition}");
        Debug.Log($"[MoveHandler] 系統強制移動: {piece.Type} 字典索引 {from} -> {to}");

        if (!_board.MovePiece(from, to))
        {
            Debug.LogError($"[MoveHandler] 強制移動失敗！");
            return false;
        }

        // 統一派發事件！外部呼叫者完全不用操心 Signal 的事
        _eventDispatcher.Dispatch(_board.DomainEvents);
        _board.ClearEvents();

        return true;
    }

    public bool ForceCapture(PiecePosition position, ChessPiece capturer = null)
    {
        Debug.Log($"[MoveHandler] 系統強制吃子: 位於 {position}");

        if (!_board.CapturePiece(position, capturer))
        {
            Debug.LogError($"[MoveHandler] 強制吃子失敗！");
            return false;
        }

        // 統一派發事件！
        _eventDispatcher.Dispatch(_board.DomainEvents);
        _board.ClearEvents();

        return true;
    }
}
