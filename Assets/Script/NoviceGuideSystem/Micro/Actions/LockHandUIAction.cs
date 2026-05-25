using UnityEngine;

[CreateAssetMenu(fileName = "LockHandUIAction", menuName = "Tutorial/Action/LockHandUI")]
public class LockHandUIAction : MicroActionSO
{
    [SerializeField] private bool lockUI = true;

    public override void Execute(SceneGameActionContext context)
    {
        if (lockUI)
        {
            context.Global.TutorialOverrideService.LockFeature("HandUI");
            Debug.Log("HandUI locked");
        }
        else
        {
            context.Global.TutorialOverrideService.UnlockFeature("HandUI");
            Debug.Log("HandUI unlocked");
        }
    }
}
