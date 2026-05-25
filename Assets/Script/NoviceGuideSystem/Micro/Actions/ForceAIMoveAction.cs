using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/Force AI Move")]
public class ForceAIMoveAction : MicroActionSO
{
    public string uciMoveCommand; // 例如 "e2e4"

    public override void Execute(SceneGameActionContext context)
    {
        // 假設你的 context 裡可以拿到 DI Container 或直接拿到 Service
        var overrideService = context.Global.TutorialOverrideService;
        overrideService.EnqueueForcedAIMove(uciMoveCommand);
    }
}