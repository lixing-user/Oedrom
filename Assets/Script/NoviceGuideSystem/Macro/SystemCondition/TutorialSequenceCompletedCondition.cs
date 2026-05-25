using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Conditions/Macro/TutorialSequenceCompleted")]
public class TutorialSequenceCompletedCondition : MacroConditionSO
{
    public string SequenceId;

    public override bool IsConditionMet(ProjectGameActionContext context)
    {
        var progressService = context.TutorialProgressService;
        return progressService != null && progressService.IsStepCompleted(SequenceId);
    }
}
