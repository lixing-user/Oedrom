using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Conditions/Macro/SessionStateCondition")]
public class SessionStateCondition : MacroConditionSO
{
    [Header("目標遊戲狀態")]
    public GameSessionState TargetState;


    [Header("目標戰鬥類型 (僅當 TargetState 為 Battle 時有效)")]
    [ShowIf(nameof(IsBattleStateSelected))]
    public BattleType TargetBattleType;

    private bool IsBattleStateSelected => TargetState == GameSessionState.Battle;


    public override bool IsConditionMet(ProjectGameActionContext context)
    {
        // 這裡不需要 OnStart 註冊事件，因為 QuestManager 每幀都會 Evaluate 活躍中的任務
        var stateMachine = context.GameSessionStateMachine;
        if (stateMachine == null) return false;

        if (context.SceneTransitionTracker.IsTransitioning)
        {
            return false;
        }

        // 1. 先判斷主要狀態是否吻合
        if (stateMachine.CurrentState != TargetState)
        {
            return false;
        }

        // 2. 如果目標狀態是 Battle，我們需要進一步驗證 BattleType
        if (TargetState == GameSessionState.Battle)
        {
            // 呼叫我們接下來要在 GameSessionStateMachine 新增的公開方法
            var currentBattleType = stateMachine.GetCurrentBattleType();

            // 如果當前的 BattleType 與我們設定的目標不符，則條件不滿足
            if (currentBattleType != TargetBattleType)
            {
                return false;
            }
        }

        return true;
    }
}