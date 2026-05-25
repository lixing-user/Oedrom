using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Conditions/FlagNotMet")]
public class FlagNotMetCondition : MacroConditionSO
{
    [Header("只要沒有這個 Flag，條件就成立")]
    [Tooltip("例如填入：QuestDone_Quest_01_Prologue")]
    public string targetFlag;

    public override bool IsConditionMet(ProjectGameActionContext context)
    {
        // 假設你的 TutorialProgressService 已泛化為處理 Flag
        var flagService = context.TutorialProgressService;
        if (flagService == null) return false;

        // 如果「沒有」這個標記，代表是新遊戲/任務尚未執行，回傳 true
        return !flagService.IsStepCompleted(targetFlag);
    }
}