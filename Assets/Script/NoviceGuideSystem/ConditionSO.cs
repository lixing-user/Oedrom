using UnityEngine;

[CreateAssetMenu(fileName = "ConditionSO", menuName = "Scriptable Objects/MacroConditionSO")]
// 給 Quest 解鎖/完成條件用的全域條件 (例如：玩家等級達標、特定任務已完成)
public abstract class MacroConditionSO : ScriptableObject
{
    public abstract bool IsConditionMet(ProjectGameActionContext context);
}


[CreateAssetMenu(fileName = "ConditionSO", menuName = "Scriptable Objects/microConditionSO")]
// 給 TutorialStep 推進用的場景條件 (例如：玩家點擊了特定 UI、棋子移動到特定格子)
public abstract class MicroConditionSO : ScriptableObject
{
    // 當這個教學步驟「剛開始」時呼叫 (適合用來註冊事件)
    public virtual void OnStart(SceneGameActionContext context) { }

    public abstract bool IsConditionMet(SceneGameActionContext context);

    // 當這個教學步驟「結束或被中斷」時呼叫 (適合用來解除事件，確保不殘留)
    public virtual void OnExit(SceneGameActionContext context) { }
}
