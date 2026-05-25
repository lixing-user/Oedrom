using ChessPieceCardProduction;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class HandDeckDTO
{
    public List<CardDTO> cards = new();
    public int maxSize;

    public HandDeckDTO() { }


    // 眖烩办家锣传
    public HandDeckDTO(HandDeck handDeck)
    {
        maxSize = handDeck.MaxSize;
        cards = handDeck.Cards.Select(card => new CardDTO(card)).ToList();
    }

    // 锣传烩办家
    public HandDeck ToDomain()
    {
        var handDeck = new HandDeck(maxSize);

        foreach (var cardDto in cards)
        {
            try
            {
                var card = cardDto.ToDomain();
                handDeck.AddCard(card);
            }
            catch (InvalidOperationException ex)
            {
                UnityEngine.Debug.LogWarning($"礚猭確礟 {cardDto.cardId}: {ex.Message}");
            }
        }

        return handDeck;
    }
}
