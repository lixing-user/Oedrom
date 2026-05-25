using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Conditions/FlagMet")]
public class FlagMetCondition : MacroConditionSO
{
    [Header("必須擁有這個 Flag，條件才成立")]
    public string requiredFlag;

    public override bool IsConditionMet(ProjectGameActionContext context)
    {
        // 檢查是否擁有該進度/標籤
        return context.TutorialProgressService.IsStepCompleted(requiredFlag);
    }
}
