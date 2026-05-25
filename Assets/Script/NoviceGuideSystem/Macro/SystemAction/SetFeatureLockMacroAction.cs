using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Actions/Macro/SetFeatureLock")]
public class SetFeatureLockMacroAction : MacroActionSO
{
    [Tooltip("要封印的功能名稱，例如 'StandbyZone'")]
    public string featureName = "StandbyZone";

    [Tooltip("True為上鎖，False為解鎖")]
    public bool isLocked = true;

    public override void Execute(ProjectGameActionContext context)
    {
        if (isLocked)
        {
            context.TutorialOverrideService.LockFeature(featureName);
        }
        else
        {
            context.TutorialOverrideService.UnlockFeature(featureName);
        }
    }
}


public class FeatureLockChangedSignal
{
    public string FeatureName { get; }
    public bool IsLocked { get; }

    public FeatureLockChangedSignal(string featureName, bool isLocked)
    {
        FeatureName = featureName;
        IsLocked = isLocked;
    }
}