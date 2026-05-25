using ChessPieceCardProduction;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

// Application/Hand/HandService.cs
public class HandService : IHandService
{
    [Inject] private HandDeck _hand;
    [Inject] private IDomainEventDispatcher _eventDispatcher;
    [Inject] private IResourceManager _resourceManager;
    [Inject] private ICardDataFactory _cardFactory;

    #region Hand Management (Application Logic)
    public void AddCard(HandCard card)
    {
        if (!CanAddCard())
        {
            Debug.LogWarning("手牌已滿，無法添加卡牌");
            return;
        }

        _hand.AddCard(card);

        _eventDispatcher.Dispatch(_hand.DomainEvents);
        _hand.ClearEvents();
    }

    public void RemoveCard(CardID cardId, RemovalReason reason)
    {
        _hand.RemoveCard(cardId, reason);

        _eventDispatcher.Dispatch(_hand.DomainEvents);
        _hand.ClearEvents();
    }

    public void RepairCard(CardID cardId)
    {
        _hand.RepairCard(cardId);

        _eventDispatcher.Dispatch(_hand.DomainEvents);
        _hand.ClearEvents();
    }


    public void EnforceHandLimit()
    {
        int newMaxSize = GetMaxHandSize();
        _hand.UpdateMaxSize(newMaxSize);

        _eventDispatcher.Dispatch(_hand.DomainEvents);
        _hand.ClearEvents();
    }

    public void ResetHand()
    {
        var cards = _hand.Cards.ToList();

        foreach (var card in cards)
        {
            _hand.RemoveCard(card.CardId, RemovalReason.Discarded);
        }

        _eventDispatcher.Dispatch(_hand.DomainEvents);
        _hand.ClearEvents();
    }
    #endregion

    #region 重置與快照機制 (Puzzle & Tutorial)
    // 建立快照：在進入戰鬥/教學的最一開始呼叫
    public List<CardDTO> CreateHandSnapshot()
    {
        return _hand.Cards.Select(card => new CardDTO(card)).ToList();
    }

    // 還原快照：在宣告失敗並重新佈陣時呼叫
    public void RestoreFromSnapshot(List<CardDTO> snapshotCards)
    {
        if (snapshotCards == null) return;

        // 1. 靜默清空當前手牌 (傳入 SystemReset)
        var currentCards = _hand.Cards.ToList();
        foreach (var card in currentCards)
        {
            _hand.RemoveCard(card.CardId, RemovalReason.SystemReset);
        }
        _eventDispatcher.Dispatch(_hand.DomainEvents); // 通知 UI 瞬間銷毀舊卡牌
        _hand.ClearEvents();

        // 2. 透過 Factory 重建領域實體
        foreach (var dto in snapshotCards)
        {
            try
            {
                // 完美利用您已經寫好的 CardDataFactory
                var card = _cardFactory.CreateFromDTO(dto);
                _hand.AddCard(card);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"還原卡牌失敗: {ex.Message}");
            }
        }
        _hand.ClearEvents();

        Debug.Log($"[HandService] 已成功從快照還原 {snapshotCards.Count} 張手牌。");
    }
    #endregion

    #region Query Methods (for UI and other systems)
    public bool CanAddCard() => _hand.Cards.Count < _hand.MaxSize;
    public IReadOnlyList<HandCard> GetHandCards() => _hand.Cards;

    // 業務規則：依人氣決定手牌上限  
    public int GetMaxHandSize()
    {
        float popularity = _resourceManager.GetVariable(VariableType.popularity);
        return Mathf.FloorToInt(6 + popularity);
    }

    public int GetCurrentHandCount()
    {
        return _hand.Cards.Count;
    }

    public int GetNormalCardCount()
    {
        // 篩選出 GetStatus() 為 normal 的數量
        return _hand.Cards.Count(card => card.GetStatus() == CardStatus.normal);
    }

    public int GetBrokenCardCount()
    {
        // 篩選出 GetStatus() 為 broken 的數量
        return _hand.Cards.Count(card => card.GetStatus() == CardStatus.broken);
    }
    #endregion
}



public interface IHandService
{
    IReadOnlyList<HandCard> GetHandCards();

    void AddCard(HandCard card);
    void RemoveCard(CardID cardId, RemovalReason reason);
    void RepairCard(CardID cardId);
    void ResetHand();
    bool CanAddCard();

    List<CardDTO> CreateHandSnapshot();
    void RestoreFromSnapshot(List<CardDTO> snapshotCards);

    int GetMaxHandSize();
    int GetCurrentHandCount();
    int GetNormalCardCount();
    int GetBrokenCardCount();
}