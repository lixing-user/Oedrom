using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Condition/Micro/PreviewMove")]
public class PreviewMoveCondition : MicroConditionSO
{
    [Header("預期預覽的目標位置")]
    public Vector2Int TargetPosition;

    [Header("預期移動的兵種 (防止移錯棋子)")]
    public ChessPieceType ExpectedPieceType;

    // --- 內部狀態 (不序列化) ---
    [NonSerialized] private bool _isConditionMet;
    [NonSerialized] private bool _hasSubscribed;
    [NonSerialized] private SceneGameActionContext _cachedContext;


    public override void OnStart(SceneGameActionContext context)
    {
        _isConditionMet = false;
        _cachedContext = context;

        // 訂閱預覽訊號
        if (!_hasSubscribed && context.SignalBus != null)
        {
            context.SignalBus.Subscribe<PreviewMoveSignal>(OnPreviewMoveFired);
            _hasSubscribed = true;
        }

        context.Global.TutorialOverrideService.SetRestrictedMove(TargetPosition);
    }

    public override bool IsConditionMet(SceneGameActionContext context)
    {
        return _isConditionMet;
    }

    public override void OnExit(SceneGameActionContext context)
    {
        context.Global.TutorialOverrideService.ClearRestrictions();

        // 確保乾淨地解除訂閱
        if (_hasSubscribed && context.SignalBus != null)
        {
            context.SignalBus.Unsubscribe<PreviewMoveSignal>(OnPreviewMoveFired);
            _hasSubscribed = false;
        }

        _isConditionMet = false;
        _cachedContext = null;
    }

    private void OnPreviewMoveFired(PreviewMoveSignal signal)
    {
        // 1. 檢查玩家預覽的「落點」是否符合預期
        if (signal.To.X != TargetPosition.x || signal.To.Y != TargetPosition.y)
            return;

        // 2. 透過 BoardQuery 確認該棋子的「兵種」是否符合預期
        // 關鍵：因為還沒正式移動，所以要去 signal.From 找這顆棋子
        var piece = _cachedContext.BoardQuery.GetPieceAt(signal.From);

        if (piece != null && piece.Type == ExpectedPieceType)
        {
            Debug.Log($"[教學系統] 玩家成功預覽移動 {ExpectedPieceType} 至 {TargetPosition}！");
            _isConditionMet = true;
        }
    }
}
