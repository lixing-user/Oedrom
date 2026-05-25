using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Condition/Micro/DialogueComplete")]
public class DialogueCompleteCondition : MicroConditionSO
{

    public override bool IsConditionMet(SceneGameActionContext context)
    {
        if (context.DialogueBoxManager == null) return false;
        // 只要狀態變成 Completed，就代表這一步達成了！
        return context.DialogueBoxManager.CurrentState == DialogueBoxState.Completed;
    }
}
