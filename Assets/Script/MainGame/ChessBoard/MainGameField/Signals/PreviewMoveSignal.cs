using System.Collections.Generic;
using UnityEngine;

// Application層的訊號：棋子預覽移動指令
public class PreviewMoveSignal
{
    public PieceID PieceId { get; set; }
    public PiecePosition From { get; set; }
    public PiecePosition To { get; set; }
}

// 觸發器
public class BatchConfirmMovesSignal 
{
    public List<PendingPieceMove> PendingMoves { get; }

    public BatchConfirmMovesSignal(List<PendingPieceMove> moves)
    {
        PendingMoves = moves;  // 快照傳遞
    }
}