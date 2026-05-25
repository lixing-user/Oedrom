using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/MarkPieceAtPosition")]
public class MarkPieceAtPositionAction : MicroActionSO
{
    [Header("要標記的棋子所在座標")]
    [Tooltip("請填寫與 CardPlacementCondition 相同的座標，以標記剛放置的棋子")]
    public Vector2Int TargetGridPosition;

    [Header("是否為主要目標")]
    public bool IsPrimaryTarget = true;

    public override void Execute(SceneGameActionContext context)
    {
        // 1. 安全檢查：確保我們有 BoardQuery 服務可以使用
        if (context.BoardQuery == null)
        {
            Debug.LogError("[MarkPieceAtPositionAction] 找不到 BoardQuery 服務，無法標記目標！");
            return;
        }

        // 2. 透過 BoardQuery 去目標位置找棋子
        var piece = context.BoardQuery.GetPieceAt(TargetGridPosition);

        // 3. 如果該位置有棋子，觸發標記信號
        if (piece != null)
        {
            Debug.Log($"[Tutorial] 成功於 {TargetGridPosition} 找到棋子，發送 MarkTargetPieceSignal。");

            context.TargetPieceTracker.RegisterTarget(piece.Id);

            context.SignalBus.Fire(new MarkTargetPieceSignal
            {
                TargetPieceId = piece.Id, // 假設你的實體有暴露 Id 屬性 (PieceID)
                IsPrimary = IsPrimaryTarget
            });
        }
        else
        {
            Debug.LogWarning($"[Tutorial] 試圖在 {TargetGridPosition} 標記棋子，但該位置沒有棋子！");
        }
    }
}
