using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/Show3DGridPointer")]
public class Show3DGridPointerAction : MicroActionSO
{
    public Vector2Int TargetGrid;

    public override void Execute(SceneGameActionContext context)
    {
        if (context.TilePointerService != null)
        {
            context.TilePointerService.ShowPointerAt(TargetGrid);
        }
    }
}
