using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/RequestBattleReset")]
public class RequestBattleResetAction : MicroActionSO
{
    public override void Execute(SceneGameActionContext context)
    {
        Debug.Log("[QuestSystem] 對話結束，發送重置信號！");
        // 發送信號給 BattleResetCoordinator
        context.Global.SignalBus.Fire(new ForceBattleResetSignal("解謎失敗回溯"));
    }
}