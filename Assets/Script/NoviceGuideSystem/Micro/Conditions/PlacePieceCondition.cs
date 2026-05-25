using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Condition/Micro/CardPlacement")]
public class CardPlacementCondition : MicroConditionSO
{
    [Header("預期放置的位置")]
    public Vector2Int TargetGridPosition;

    [Header("預期的兵種")]
    public ChessPieceType ExpectedPieceType;

    public override void OnStart(SceneGameActionContext context)
    {
        context.Global.TutorialOverrideService.SetRestrictedPlacement(TargetGridPosition);

        context.Global.TutorialOverrideService.SetRestrictedCardPlay(ExpectedPieceType);

        context.Global.TutorialOverrideService.LockAllPieceSelection();
    }

    public override bool IsConditionMet(SceneGameActionContext context)
    {
        if (context.BoardQuery == null) return false;

        // 1. 透過 BoardQuery 去目標位置找棋子
        var pos = new PiecePosition(TargetGridPosition.x, TargetGridPosition.y);
        var piece = context.BoardQuery.GetPieceAt(pos);

        // 2. 如果有棋子，且種類符合我們指定的卡牌種類，就視為條件達成
        if (piece != null && piece.Type == ExpectedPieceType)
        {
            return true;
        }

        return false;
    }

    public override void OnExit(SceneGameActionContext context)
    {
        context.Global.TutorialOverrideService.ClearRestrictions();
    }
}