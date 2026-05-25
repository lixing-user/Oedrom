using ChessPieceCardProduction;
using UnityEngine;
using Zenject;

public interface ICardSelectionExtension
{
    string ExtensionName { get; }

    // 傳入 HandCard 領域模型進行判定
    bool CanSelectCard(HandCard card);
}


public class TutorialCardSelectionExtension : ICardSelectionExtension
{
    [InjectOptional] private readonly ITutorialOverrideService _overrideService;

    public string ExtensionName => "TutorialCardRestriction";

    public bool CanSelectCard(HandCard card)
    {
        // 如果沒有教學服務，預設放行
        if (_overrideService == null) return true;

        // 呼叫 OverrideService 檢查目前是否限制了特定兵種的打出
        if (_overrideService.IsCardPlayRestricted(card.CardType))
        {
            Debug.LogWarning($"[教學攔截] 目前步驟限制只能使用特定卡牌，拒絕選取: {card.CardType}");
            return false;
        }

        return true;
    }
}
