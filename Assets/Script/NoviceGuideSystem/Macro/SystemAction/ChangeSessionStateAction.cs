using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Actions/Macro/ChangeSessionState")]
public class ChangeSessionStateAction : MacroActionSO
{
    [Header("目標遊戲狀態")]
    public GameSessionState TargetState;

    [Header("目標戰鬥類型 (僅當 TargetState 為 Battle 時有效)")]
    [ShowIf(nameof(IsBattleStateSelected))]
    public BattleType TargetBattleType = BattleType.Normal;


    private bool IsBattleStateSelected => TargetState == GameSessionState.Battle;


    public override void Execute(ProjectGameActionContext context)
    {
        if (context.SceneTransitionTracker != null)
        {
            context.SceneTransitionTracker.LockTransitionSynchronously();
        }

        // 2. 根據目標狀態，發射對應的 Signal
        if (TargetState == GameSessionState.Battle)
        {
            context.SignalBus.Fire(new RequestEnterBattleSignal
            {
                BattleType = TargetBattleType,
                // 因為是從 Story 劇情強制切入，可能沒有實體的盤面互動資料，這裡傳 null
                // (除非你有特定的劇情戰鬥資料需要掛載到 PendingBattle)
                PendingBattle = null
            });
        }
        else
        {
            context.SignalBus.Fire(new RequestSessionStateChangeSignal { TargetState = TargetState });
        }
    }
}
