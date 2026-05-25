using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/HighlightValidMoves")]
public class HighlightValidMovesAction : MicroActionSO
{
    [Header("目標棋子的網格位置")]
    public Vector2Int TargetPosition;

    [Header("是否為常駐高光 (玩家點擊空白處不會消失)")]
    public bool IsPersistent = true;

    public override void Execute(SceneGameActionContext context)
    {
        // 1. 防呆與隱藏邏輯
        if (context.BoardVisualizer == null) return;

        // 2. 取得目標棋子 (透過 BoardQuery)
        var piece = context.BoardQuery?.GetPieceAt(TargetPosition);
        if (piece == null)
        {
            Debug.LogWarning($"[HighlightValidMoves] 位置 {TargetPosition} 沒有棋子！");
            return;
        }

        // 3. 找到負責棋盤邏輯的 SelectionExtension
        var boardExtension = context.SelectionExtensions?
            .FirstOrDefault(e => e is BoardPieceSelectionExtension);

        if (boardExtension != null)
        {
            // 4. 建構查詢 Context
            var selectionContext = new PieceSelectionContext
            {
                Source = PieceSelectionSource.Board,
                OriginalPosition = new PiecePosition(TargetPosition.x, TargetPosition.y) // 假設您的架構支援此轉換
            };

            // 5. 取得 Valid Moves 並繪製高光
            var validTiles = boardExtension.GetValidPlacementTiles(piece.Id, selectionContext);

            if (validTiles != null && validTiles.Count > 0)
            {
                if (IsPersistent)
                {
                    context.BoardVisualizer.SetPersistentHighlights(validTiles);
                }
                else
                {
                    context.BoardVisualizer.HighlightTiles(validTiles);
                }

                Debug.Log($"[HighlightValidMoves] 成功顯示 {piece.Id} 的 {validTiles.Count} 個移動高光。");
            }
        }
        else
        {
            Debug.LogWarning("[HighlightValidMoves] 找不到 BoardPieceSelectionExtension！");
        }
    }
}
