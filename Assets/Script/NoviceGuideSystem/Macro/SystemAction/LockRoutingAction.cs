using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Actions/Macro/LockRouting")]
public class LockRoutingAction : MacroActionSO
{
    public override void Execute(ProjectGameActionContext context) => context.SignalBus.Fire<LockRoutingSignal>();
}