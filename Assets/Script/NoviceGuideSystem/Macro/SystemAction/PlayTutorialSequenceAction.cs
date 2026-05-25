using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Actions/Macro/PlayTutorialSequence")]
public class PlayTutorialSequenceAction : MacroActionSO
{
    public string SequenceId;
    public TutorialStep[] Steps; // 放入具體的微觀步驟

    public override void Execute(ProjectGameActionContext context)
    {
        if (context.SceneTransitionTracker != null && context.SceneTransitionTracker.IsTransitioning)
        {
            // 情況 A：正在切換場景 (例如準備進戰鬥)
            // 將教學存入暫存器，等待新場景的 Runner 醒來領取
            if (context.PendingTutorialContext != null)
            {
                context.PendingTutorialContext.SetPendingSequence(SequenceId, Steps);
                Debug.Log($"[Action] 場景切換中，已將微觀教學 '{SequenceId}' 放入跨場景排程。");
            }
        }
        else
        {
            context.SignalBus.Fire(new PlayTutorialSequenceSignal
            {
                SequenceId = SequenceId,
                Steps = Steps
            });
            Debug.Log($"[Action] 已將微觀教學 '{SequenceId}' 加入排程，等待場景就緒。");
        }
           
        //context.PendingTutorialContext.SetPendingSequence(SequenceId, Steps);
    }
}