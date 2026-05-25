using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

// 移動執行策略介面
public interface IMoveExecutionStrategy
{
    void ExecuteMove(PieceID pieceId, PiecePosition from, PiecePosition to);
}

#region 策略實作
// 預覽策略(MGF
public class PreviewMoveStrategy : IMoveExecutionStrategy
{
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly IPiecePreviewVisualizerService _previewVisualizer;
    [Inject] private readonly IArrowVisualizerService _arrowService;

    public void ExecuteMove(PieceID pieceId, PiecePosition from, PiecePosition to)
    {
        _signalBus.Fire(new PreviewMoveSignal { PieceId = pieceId, From = from, To = to });

        _previewVisualizer.ShowSnappedPreview(pieceId, from, to);

        _arrowService.ClearArrows(); // 確保每次只顯示最新的移動箭頭
        _arrowService.DrawBoardArrow(from, to);

        Debug.Log($"[預覽移動] {from} → {to}");
    }
}

// 直接執行策略(Battle
public class DirectMoveStrategy : IMoveExecutionStrategy
{
    [Inject] private readonly IMoveHandler _moveHandler;

    public void ExecuteMove(PieceID pieceId, PiecePosition from, PiecePosition to)
    {
        bool success = _moveHandler.MoveTo(pieceId, to);
        Debug.Log($"[直接移動] {from} → {to}, 結果: {success}");
    }
}

// 交換策略(Battle
public class SwapMoveStrategy : IMoveExecutionStrategy
{
    [Inject] private readonly IGameBoard _board;
    [Inject] private readonly IMoveHandler _moveHandler;
    //[Inject] private readonly IMoveValidateService _validator;
    //[Inject] private readonly IDomainEventDispatcher _eventDispatcher;

    public void ExecuteMove(PieceID pieceId, PiecePosition from, PiecePosition to)
    {
        #region 舊版
        //var sourcePiece = _board.GetPieceById(pieceId);
        //var targetPiece = _board.GetPieceAt(to);

        //if (targetPiece == null || sourcePiece.IsEnemyOf(targetPiece))
        //{
        //    Debug.LogWarning($"[交換失敗] 目標位置無友軍棋子");
        //    return;
        //}

        //bool success = _board.SwapPieces(from, to);
        //if (success)
        //{
        //    // 分派領域事件
        //    _eventDispatcher.Dispatch(_board.DomainEvents);
        //    _board.ClearEvents();
        //    Debug.Log($"[交換成功] {from} <swap> {to}");
        //}
        #endregion
        bool success = _moveHandler.SwapPieces(from, to);
        Debug.Log($"[交換移動] {from} <swap> {to}, 結果: {success}");
    }
}
#endregion

#region 複合式策略（狀態機）
public class SetupMoveStrategy : IMoveExecutionStrategy
{
    [Inject] private readonly IMoveHandler _moveHandler;
    [Inject] private readonly IGameBoard _board;
    //[Inject] private readonly IDomainEventDispatcher _eventDispatcher;

    public void ExecuteMove(PieceID pieceId, PiecePosition from, PiecePosition to)
    {
        #region 舊版
        //var sourcePiece = _board.GetPieceById(pieceId);
        //var targetPiece = _board.GetPieceAt(to);

        //if (targetPiece == null)
        //{
        //    // 空格：直接調用 MoveHandler
        //    bool success = _moveHandler.MoveTo(pieceId, to);
        //    Debug.Log($"[佈陣策略] 直接移動 {from} → {to}: {success}");
        //}
        //else if (!sourcePiece.IsEnemyOf(targetPiece))
        //{
        //    // 友軍：交換
        //    bool success = _board.SwapPieces(from, to);
        //    if (success)
        //    {
        //        _eventDispatcher.Dispatch(_board.DomainEvents);
        //        _board.ClearEvents();
        //        Debug.Log($"[佈陣策略] 交換 {from} <swap> {to}");
        //    }
        //}
        #endregion
        var targetPiece = _board.GetPieceAt(to);

        if (targetPiece == null)
        {
            // 空格：直接移動
            bool success = _moveHandler.MoveTo(pieceId, to);
            Debug.Log($"[佈陣策略] 直接移動 {from} → {to}: {success}");
        }
        else
        {
            // 友軍：交換
            bool success = _moveHandler.SwapPieces(from, to);
            Debug.Log($"[佈陣策略] 交換 {from} <swap> {to}: {success}");
        }
    }
}
#endregion
