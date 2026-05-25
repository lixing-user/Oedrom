using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/ShowArrow")]
public class ShowArrowAction : MicroActionSO
{
    [Header("箭頭起點 (棋盤座標)")]
    public Vector2Int StartGrid;

    [Header("箭頭終點 (棋盤座標)")]
    public Vector2Int EndGrid;

    public override void Execute(SceneGameActionContext context)
    {
        if (context.ArrowVisualizerService != null)
        {
            var from = new PiecePosition(StartGrid.x, StartGrid.y);
            var to = new PiecePosition(EndGrid.x, EndGrid.y);

            context.ArrowVisualizerService.DrawBoardArrow(from, to);
        }
    }
}