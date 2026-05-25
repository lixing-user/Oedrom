using ChessPieceCardProduction;
using UnityEngine;


// 初始化完成（僅觸發一次）
public class CardsLoadedSignal { }


// 手牌變化（通用通知）
public class HandCardsChangedSignal { }


// 添加卡牌（包含具體數據）
public class CardAddedSignal
{
    public HandCard Card;
}


// 移除卡牌（包含 ID）
public class CardRemovedSignal
{
    public CardID CardId;
}


// 卡牌狀態變化（如破損）
public class CardStatusChangedSignal
{
    public CardID CardId;
    public CardStatus NewStatus;
}

