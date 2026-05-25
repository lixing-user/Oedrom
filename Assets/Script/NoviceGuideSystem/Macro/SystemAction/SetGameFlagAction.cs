using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Actions/SetGameFlag")]
public class SetGameFlagAction : MacroActionSO
{
    [Header("要寫入的進度標記")]
    [Tooltip("例如填入：TutorialSeq_02_Done")]
    public string targetFlag;

    public override void Execute(ProjectGameActionContext context)
    {
        Debug.Log($"[Action] 寫入遊戲進度標記: {targetFlag}");

        // 呼叫 ProgressService 寫入標記，它內部會自動發射 GameFlagAddedSignal！
        context.TutorialProgressService.MarkStepCompleted(targetFlag);
    }
}