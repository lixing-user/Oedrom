using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Actions/Macro/PlayStory")]
public class PlayStoryAction : MacroActionSO
{
    public StoryChapterConfig StoryConfig;

    public override void Execute(ProjectGameActionContext context)
    {
        var storyManager = context.StoryProgressManager;
        if (storyManager != null)
        {
            storyManager.SetCurrentChapter(StoryConfig);
            context.SignalBus.Fire(new RequestSessionStateChangeSignal { TargetState = GameSessionState.Story });
        }
    }
}
