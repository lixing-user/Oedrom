using ChessPieceCardProduction;
using UnityEngine;

public enum RemovalReason
{
    Played,        // 卡牌被使用
    Sold,          // 卡牌被賣出
    Discarded,     // 手動丟棄
    ForcedDiscard,  // 因上限強制丟棄
    SystemReset   // 其他系統觸發的移除（如教學回溯）
}

public class CardAddedEvent : DomainEvent
{
    public HandCard Card;
    public ChessPieceType CardType { get; }
    public Team Team { get; }
    public int CurrentHandSize { get; }

    public CardAddedEvent(HandCard card, ChessPieceType type, Team team, int currentSize)
    {
        Card = card;
        CardType = type;
        Team = team;
        CurrentHandSize = currentSize;
    }
}

public class CardRemovedEvent : DomainEvent
{
    public CardID CardId { get; }
    public RemovalReason Reason { get; }
    public int RemainingHandSize { get; }

    public CardRemovedEvent(CardID cardId, RemovalReason reason, int remaining)
    {
        CardId = cardId;
        Reason = reason;
        RemainingHandSize = remaining;
    }
}

public class CardDiscardedEvent : DomainEvent
{
    public CardID CardId { get; }
    public ChessPieceType CardType { get; }

    public CardDiscardedEvent(CardID cardId, ChessPieceType type)
    {
        CardId = cardId;
        CardType = type;
    }
}

public class HandSizeLimitChangedEvent : DomainEvent
{
    public int OldLimit { get; }
    public int NewLimit { get; }

    public HandSizeLimitChangedEvent(int oldLimit, int newLimit)
    {
        OldLimit = oldLimit;
        NewLimit = newLimit;
    }
}

public class CardStatusChangedEvent : DomainEvent
{
    public CardID CardId { get; }
    public CardStatus NewStatus { get; }

    public CardStatusChangedEvent(CardID cardId, CardStatus status)
    {
        CardId = cardId;
        NewStatus = status;
    }
}