using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Condition/Micro/MovePiece")]
public class MovePieceCondition : MicroConditionSO
{
    [Header("預期移動的目標位置")]
    public Vector2Int TargetPosition;

    [Header("預期移動的兵種 (防止移錯棋子)")]
    public ChessPieceType ExpectedPieceType;

    public override bool IsConditionMet(SceneGameActionContext context)
    {
        if (context.BoardQuery == null) return false;

        // 透過 Context 去查 BoardQueryService，確認目標位置有沒有棋子
        var targetPos = new PiecePosition(TargetPosition.x, TargetPosition.y);
        var piece = context.BoardQuery.GetPieceAt(TargetPosition);

        if (piece != null && piece.Type == ExpectedPieceType)
        {
            return true;
        }

        return false;
    }
}
