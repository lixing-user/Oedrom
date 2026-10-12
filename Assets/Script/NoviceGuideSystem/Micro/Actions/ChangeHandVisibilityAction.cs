using UnityEngine;

[CreateAssetMenu(fileName = "ChangeHandVisibilityAction", menuName = "Tutorial/Action/ChangeHandVisibility")]
public class ChangeHandVisibilityAction : MicroActionSO
{
    [SerializeField] private bool isSubtle = true;

    public override void Execute(SceneGameActionContext context)
    {
        if (isSubtle)
        {
            context.SignalBus.Fire(new ChangeHandUIVisibilitySignal
            {
                VisibilityMode = UIVisibilityHeadupMode.Subtle,
                DisplayMode = HandDisplayMode.BottomRight
            });
        }
        else
        {
            context.SignalBus.Fire(new ChangeHandUIVisibilitySignal
            {
                VisibilityMode = UIVisibilityHeadupMode.Standard,
                DisplayMode = HandDisplayMode.Centered
            });
        }
    }
}
