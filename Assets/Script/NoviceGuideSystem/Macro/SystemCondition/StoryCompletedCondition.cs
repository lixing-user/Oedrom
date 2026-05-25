using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Conditions/Macro/StoryCompleted")]
public class StoryCompletedCondition : MacroConditionSO
{
    public string ChapterId;

    public override bool IsConditionMet(ProjectGameActionContext context)
    {
        // 需要實作一個 Flag 系統，或是向 StoryProgressManager 查詢該 ChapterId 是否已閱讀
        return context.StoryProgressManager?.IsChapterCompleted(ChapterId) ?? false;
    }
}
