using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Actions/Macro/UnlockRouting")]
public class UnlockRoutingAction : MacroActionSO
{
    public override void Execute(ProjectGameActionContext context) => context.SignalBus.Fire<UnlockRoutingSignal>();
}
