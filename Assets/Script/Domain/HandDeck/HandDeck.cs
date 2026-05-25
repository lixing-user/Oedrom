using ChessPieceCardProduction;
using System;
using System.Collections.Generic;
using System.Linq;


// Domain/ Aggregate Root/ HandDeck.cs
public class HandDeck 
{
    private readonly List<HandCard> _handCards = new();
    private int _maxSize;

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    public HandDeck(int maxSize) => _maxSize = maxSize;


    #region 領域邏輯
    public void AddCard(HandCard card)
    {
        if (_handCards.Count >= _maxSize)
            throw new InvalidOperationException("手牌已滿");

        _handCards.Add(card);

        _domainEvents.Add(new CardAddedEvent(
            card, 
            card.CardType, 
            card.Team, 
            _handCards.Count));
    }

    public void RemoveCard(CardID cardId, RemovalReason reason)
    {
        var card = _handCards.FirstOrDefault(c => c.CardId == cardId);
        if (card == null)
            throw new InvalidOperationException("卡牌不存在");

        _handCards.Remove(card);

        _domainEvents.Add(new CardRemovedEvent(
            cardId, 
            reason, 
            _handCards.Count));
    }

    public void RepairCard(CardID cardId)
    {
        var card = _handCards.FirstOrDefault(c => c.CardId == cardId);
        if (card == null)
            throw new InvalidOperationException("卡牌不存在");

        card.Repair();

        _domainEvents.Add(new CardStatusChangedEvent(
            cardId,
            CardStatus.normal
        ));
    }
    #endregion

    #region 業務邏輯
    public void UpdateMaxSize(int newMaxSize)
    {
        if (newMaxSize == _maxSize) return;

        int oldMaxSize = _maxSize;
        _maxSize = newMaxSize;

        _domainEvents.Add(new HandSizeLimitChangedEvent(oldMaxSize, newMaxSize));
        EnforceLimit();
    }
    private void EnforceLimit()
    {
        while (_handCards.Count > _maxSize)
        {
            var discarded = _handCards[0];
            _handCards.RemoveAt(0);

            _domainEvents.Add(new CardDiscardedEvent(
                discarded.CardId,
                discarded.CardType));
        }
    }

    public void BreakCard(CardID cardId)
    {
        var card = _handCards.FirstOrDefault(c => c.CardId == cardId);
        if (card == null)
            throw new InvalidOperationException("卡牌不存在");

        card.SetBroken(); // Entity 內部方法

        _domainEvents.Add(new CardStatusChangedEvent(
            cardId,
            CardStatus.broken
        ));
    }
    #endregion


    public void ClearEvents() => _domainEvents.Clear();

    public IReadOnlyList<HandCard> Cards => _handCards.AsReadOnly();

    public int MaxSize => _maxSize;
}
