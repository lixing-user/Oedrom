using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Condition/Micro/EscapeSuccess")]
public class EscapeSuccessCondition : MicroConditionSO
{
    [Header("逃脫的目標兵種 (預設為皇后)")]
    public ChessPieceType EscaperType = ChessPieceType.Queen;

    [Header("逃脫的目標陣營 (預設為白方)")]
    public Team EscaperTeam = Team.White;

    public override bool IsConditionMet(SceneGameActionContext context)
    {
        if (context.BoardQuery == null) return false;

        // 遍歷所有定義在靜態配置 (Setup) 中的逃脫點
        foreach (var escapePoint in EscapeBattleSetup.EscapePoint)
        {
            var targetPos = new PiecePosition(escapePoint.X, escapePoint.Y);
            var pieceAtTarget = context.BoardQuery.GetPieceAt(targetPos);

            // 檢查逃脫點上是否有棋子，且陣營與兵種完全符合預期
            if (pieceAtTarget != null &&
                pieceAtTarget.Team == EscaperTeam &&
                pieceAtTarget.Type == EscaperType)
            {
                Debug.Log($"[EscapeCondition] 條件達成：{EscaperTeam} {EscaperType} 已抵達逃脫點 ({escapePoint.X}, {escapePoint.Y})！");
                return true; // 只要踩到任何一個有效逃脫點，即視為條件滿足
            }
        }

        return false; // 尚未抵達
    }
}