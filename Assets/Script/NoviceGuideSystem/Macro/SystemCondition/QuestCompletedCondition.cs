using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Conditions/Macro/QuestCompleted")]
public class QuestCompletedCondition : MacroConditionSO
{
    [Header("要檢查是否完成的任務 ID")]
    [Tooltip("對應到目標 QuestSO 的 questId")]
    public string TargetQuestId;

    public override bool IsConditionMet(ProjectGameActionContext context)
    {
        // 防呆：如果找不到 Flag 服務，就代表無法確認，回傳 false
        if (context.TutorialProgressService == null)
        {
            Debug.LogWarning("[QuestCompletedCondition] 找不到 TutorialProgressService，無法評估條件。");
            return false;
        }

        // 當一個任務完成時，會寫入一個名為 "QuestDone_{QuestId}" 的 Flag
        string questCompleteFlag = $"QuestDone_{TargetQuestId}";

        // 檢查這個 Flag 是否存在
        return context.TutorialProgressService.IsStepCompleted(questCompleteFlag);
    }
}
