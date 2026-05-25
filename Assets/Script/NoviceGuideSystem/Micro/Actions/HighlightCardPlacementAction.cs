using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/HighlightCardPlacement")]
public class HighlightCardPlacementAction : MicroActionSO
{
    [Header("是否為常駐高光 (不受玩家拖曳卡牌中斷)")]
    public bool IsPersistent = true;

    public override void Execute(SceneGameActionContext context)
    {
        if (context.BoardVisualizer == null || context.CardPlacementRule == null)
        {
            Debug.LogWarning("[HighlightCardPlacement] 缺少 BoardVisualizer 或 CardPlacementRule！");
            return;
        }

        // 1. 直接向領域規則 (Rule) 索取當前合法的所有落子點
        // 注意：因為你的 BottomRowPlacementRule 內部已經實作了 ITutorialOverrideService 的檢測
        // 所以如果教學系統有鎖定特定格子，這裡拿到的清單會自動被過濾！
        var validTiles = context.CardPlacementRule.GetValidPlacementTiles();

        if (validTiles != null && validTiles.Count > 0)
        {
            // 2. 呼叫視覺系統進行渲染
            if (IsPersistent)
            {
                context.BoardVisualizer.SetPersistentHighlights(validTiles);
                Debug.Log($"[HighlightCardPlacement] 已顯示 {validTiles.Count} 個常駐卡牌放置高光。");
            }
            else
            {
                context.BoardVisualizer.HighlightTiles(validTiles);
            }
        }
    }
}