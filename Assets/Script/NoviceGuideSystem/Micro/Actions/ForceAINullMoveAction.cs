using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/Force AI Piece In Place")]
public class ForceAINullMoveAction : MicroActionSO
{
    public Team targetTeam = Team.Black; // 預設操作黑方(AI)國王
    public ChessPieceType pieceType = ChessPieceType.King; // 預設操作國王

    public override void Execute(SceneGameActionContext context)
    {
        // 2. 欺騙狀態機 (注入無效的 UCI 讓 AIProvider 回傳空指令)
        if (targetTeam == Team.Black)
        {
            context.Global.TutorialOverrideService.EnqueueForcedAIMove("skip_turn");
            Debug.Log($"[Tutorial] 已安插跳過指令，攔截 AI 引擎運算");
        }


        // 3. 找出指定類型的棋子
        var piece = context.BoardQuery.GetPiecesForTeam(targetTeam)
                               .FirstOrDefault(p => p.Type == pieceType);

        if (piece == null)
        {
            Debug.LogWarning($"[Tutorial] 找不到 {targetTeam} 的 {pieceType}！");
            return;
        }

        var currentPos = piece.CurrentPosition;
        Debug.Log($"[Tutorial] 強制 {targetTeam} 的 {pieceType} 於 {currentPos} 執行原地移動！");


        // 4. 強制執行領域移動
        bool success = context.MoveHandler.ForceMove(piece.Id, currentPos);

        if (!success)
        {
            Debug.LogError("[Tutorial] 強制原地移動失敗！");
        }
    }
}
