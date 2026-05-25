using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Conditions/Macro/GameFlowCondition")]
public class GameFlowCondition : MacroConditionSO
{
    [Header("目標遊戲流程")]
    public GameFlow TargetFlow; // 例如設定為 GameFlow.PlayerTurn

    public override bool IsConditionMet(ProjectGameActionContext context)
    {
        // 1. 防呆：如果現在根本不在 MainGame 狀態，直接回傳 false
        if (context.GameSessionStateMachine.CurrentState != GameSessionState.MainGame)
        {
            return false;
        }

        // 2. 防呆：如果是場景切換中，也回傳 false (避免拿到舊資料)
        if (context.SceneTransitionTracker.IsTransitioning)
        {
            return false;
        }

        // 3. 透過全域追蹤器，取得目前的 Flow
        return context.GlobalFlowStateTracker.CurrentFlow == TargetFlow;
    }
}
