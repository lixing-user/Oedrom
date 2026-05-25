using System.Linq;
using UnityEngine;
using Zenject;

[CreateAssetMenu(menuName = "Tutorial/Action/Micro/HighlightUI")]
public class HighlightUIAction : MicroActionSO
{
    //[Header("要引導玩家點擊的 UI 元素")]
    //public ChessPieceType TargetType;

    [Header("填入你要對準的標靶 ID (例如: Target_SpawnKnight)")]
    public string TargetID;

    public override void Execute(SceneGameActionContext context)
    {
        if (context.TargetRegistry == null || context.TutorialHighlightService == null)
        {
            Debug.LogWarning("[HighlightCardAction] 找不到必要的 UI 服務，略過高光演出。");
            return;
        }

        // 1. 去註冊中心把你在 Editor 擺好的隱形標靶拿出來
        RectTransform dummyTarget = context.TargetRegistry.GetTarget(TargetID);

        if (dummyTarget != null)
        {
            // 2. 直接把這個隱形標靶當作「UI 卡牌」餵給 Unmask 的 FitTarget 功能！
            context.TutorialHighlightService.HighlightUIElement(dummyTarget);
        }
        else
        {
            Debug.LogError($"[教學系統] 找不到 ID 為 {TargetID} 的隱形標靶！");
        }
    }
}
