using UnityEngine;

[CreateAssetMenu(fileName = "ActionSO", menuName = "Scriptable Objects/MacroActionSO")]
// 給 QuestManager 用的全域動作
public abstract class MacroActionSO : ScriptableObject
{
    public abstract void Execute(ProjectGameActionContext context);
}


[CreateAssetMenu(fileName = "ActionSO", menuName = "Scriptable Objects/microActionSO")]
// 給 TutorialStepRunner 用的場景動作
public abstract class MicroActionSO : ScriptableObject
{
    public abstract void Execute(SceneGameActionContext context);
}