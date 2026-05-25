using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/InstantVictoryAndResolve")]
public class InstantVictoryAndResolveAction : MicroActionSO
{
    [Tooltip("勝利時發送的原因訊息")]
    public string victoryReason = "完成解謎步驟！";

    public override void Execute(SceneGameActionContext context)
    {
        Debug.Log("[MicroAction] 步驟目標達成！提早解鎖 UI 並發送勝利結算！");

        // 1. 搶先解鎖 UI，確保海關 (Gatekeeper) 會放行
        context.Global.TutorialOverrideService.UnlockFeature("GameOverUI");

        // 2. 瞬間發送勝利信號，通知海關與呈現層 (Presenter)
        context.Global.SignalBus.Fire(new GameEndedSignal(Team.White, victoryReason));
    }
}
