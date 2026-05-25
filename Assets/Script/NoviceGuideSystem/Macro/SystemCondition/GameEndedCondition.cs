using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Conditions/GameEndedCondition")]
public class GameEndedCondition : MacroConditionSO
{
    [Header("目標結局原因")]
    [Tooltip("例如：'護衛目標陣亡' 或 '黑方國王被擊敗'")]
    public List<string> TargetReasons = new List<string>();

    [Header("目標獲勝隊伍 (選填)")]
    public Team TargetWinner = Team.Black;

    public override bool IsConditionMet(ProjectGameActionContext context)
    {
        var tracker = context.MGFContext;
        if (tracker == null) return false;

        // 檢查原因是否在列表中 (使用 List.Contains)
        bool isReasonMatched = TargetReasons.Contains(tracker.LastReason);

        // 檢查獲勝隊伍是否匹配
        bool isWinnerMatched = tracker.LastWinner == TargetWinner;

        return isReasonMatched && isWinnerMatched;
    }
}