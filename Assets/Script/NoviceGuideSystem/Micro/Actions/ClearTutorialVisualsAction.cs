using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/ClearAllVisuals")]
public class ClearTutorialVisualsAction : MicroActionSO
{
    [Header("是否同時關閉對話框？")]
    public bool CloseDialogue = true;

    public override void Execute(SceneGameActionContext context)
    {
        // 1.清除所有遮罩與高光
        if (context.TutorialHighlightService != null)
        {
            context.TutorialHighlightService.ClearAllHighlights();
        }
        if (context.ArrowVisualizerService != null)
        {
            context.ArrowVisualizerService.ClearArrows();
        }
        if (context.TilePointerService != null)
        {
            context.TilePointerService.ClearPointer();
        }
        if (context.BoardVisualizer != null)
        {
            context.BoardVisualizer.RemoveHighlightTiles();
            context.BoardVisualizer.ClearPersistentHighlights();
        }


        Debug.Log("[教學系統] 已清理上一個步驟的所有視覺指引。");
    }
}