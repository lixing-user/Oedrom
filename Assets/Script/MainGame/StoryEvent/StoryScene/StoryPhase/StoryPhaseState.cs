using UnityEngine;

public class ProloguePhase : IStoryPhaseState
{
    public void OnEnter(StoryStateMachine machine)
    => machine.GetPresenter().LoadChapter(machine.GetCurrentChapter());
    public void OnUpdate(StoryStateMachine machine) { }
    public void OnExit(StoryStateMachine machine) { }
}

public class MainDialoguePhase : IStoryPhaseState
{
    public void OnEnter(StoryStateMachine machine)
    => machine.GetPresenter().LoadChapter(machine.GetCurrentChapter());
    public void OnUpdate(StoryStateMachine machine) { }
    public void OnExit(StoryStateMachine machine) { }
}

public class EpiloguePhase : IStoryPhaseState
{
    public void OnEnter(StoryStateMachine machine)
    => machine.GetPresenter().LoadChapter(machine.GetCurrentChapter());
    public void OnUpdate(StoryStateMachine machine) { }
    public void OnExit(StoryStateMachine machine) { }
}
