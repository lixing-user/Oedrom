using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/FocusCamera")]
public class FocusCameraAction : MicroActionSO
{
    [Header("目標棋子的網格位置")]
    public Vector2Int TargetPosition;

    public override void Execute(SceneGameActionContext context)
    {
        if (context.BoardQuery == null || context.PieceViewRegistry == null)
        {
            Debug.LogWarning("[FocusCameraOnPieceAction] 缺少必要的場景服務，無法執行聚焦。");
            return;
        }

        // 1. 查詢該位置上的棋子邏輯資料
        var piece = context.BoardQuery.GetPieceAt(TargetPosition);

        if (piece != null)
        {
            // 2. 透過 PieceViewRegistry 取得該棋子在場景上的實際 View (GameObject)
            var pieceView = context.PieceViewRegistry.GetView(piece.Id);
            if (pieceView != null)
            {
                // 3. 發送 Signal，請 CameraPresenter 把攝影機轉過去
                context.Global.SignalBus.Fire(new CameraFocusRequestSignal(pieceView.transform));
            }
            else
            {
                Debug.LogWarning($"[FocusCameraAction] 找不到棋子 {piece.Id} 的 View！");
            }
        }
        else
        {
            Debug.LogWarning($"[FocusCameraAction] 位置 {TargetPosition} 上沒有棋子可供聚焦！");
        }
    }
}
