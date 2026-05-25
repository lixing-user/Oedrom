using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Condition/Micro/TakeComponent")]
public class TakeBattleSessionComponentCondition : MicroConditionSO
{
    public override bool IsConditionMet(SceneGameActionContext context)
    {
        // 1. 取得當前的戰鬥資料
        var battle = context.Global.BattleContext?.CurrentBattle;
        if (battle == null) return false;

        // 2. 找出戰鬥中屬於「黑方」的目標 PieceID
        PieceID targetId = null;
        if (battle.AttackerTeam == Team.Black)
        {
            targetId = battle.AttackerId;
        }
        else if (battle.DefenderTeam == Team.Black)
        {
            targetId = battle.DefenderId;
        }
        if (targetId == null) return false;

        // 3. 獲取目前場上所有還存活的黑方棋子
        var aliveBlackPieces = context.BoardQuery.GetPiecesByTeam(Team.Black);

        // 4. 檢查該目標是否還在存活名單內
        bool isTargetStillAlive = aliveBlackPieces.Any(piece => piece.Id == targetId);

        // 如果在存活名單中「找不到」它了，代表已經被玩家吃掉，條件成立！
        return !isTargetStillAlive;
    }
}

