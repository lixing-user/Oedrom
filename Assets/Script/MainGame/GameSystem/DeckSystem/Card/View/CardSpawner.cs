using ChessPieceCardProduction;
using System;
using System.Collections.Generic;
using System.Linq;
using Systems.SceneManagement;
using UnityEngine;
using Zenject;


public interface ICardSpawner
{
    // 方法 1：創建新卡牌（領域模型 + 視圖）
    HandCard SpawnCard(ChessPieceType type, Team team, GameMode mode);

    // 方法 2：為已存在的 HandCard 創建視圖（重載）
    CardFacade SpawnCardView(HandCard handCard, GameMode mode);

    HandCard SpawnCardFromPiece(ChessPiece piece, GameMode mode);
}



public class CardSpawner : ICardSpawner
{
    [Inject] private readonly ICardFactory _facadeFactory;
    [Inject] private readonly ICardDataFactory _dataFactory;


    #region 業務邏輯
    // 創建新卡牌（領域模型 + 視圖）
    public HandCard SpawnCard(ChessPieceType type, Team team, GameMode mode)
    {
        // 1. 使用 CardDataFactory 創建領域模型
        HandCard card = _dataFactory.CreateCard(type, team);

        // 2. 創建視圖
        CreateCardView(card, mode);

        // 3. 返回領域模型
        return card;
    }

    // 只創建 Domain
    public HandCard SpawnCardFromPiece(ChessPiece piece, GameMode mode)
    {
        #region 舊版實作
        //if (piece == null)
        //    throw new System.ArgumentNullException(nameof(piece));

        //// 1. 使用 CardDataFactory 創建（帶來源追蹤）
        //HandCard card = _dataFactory.CreateCardFromPiece(piece);

        //// 2. 創建視圖
        //CreateCardView(card, mode);

        //// 3. 返回領域模型
        //Debug.Log($"[CardSpawner] 從棋子創建卡牌: {piece.Type} → {card.CardId}");
        //return card;
        #endregion
        return _dataFactory.CreateCardFromPiece(piece);
    }

    // 只生成視圖（現有的卡牌），不創建領域模型
    public CardFacade SpawnCardView(HandCard handCard, GameMode mode)
    {
        if (handCard == null)
        {
            Debug.LogError("[CardSpawner] HandCard 為 null");
            return null;
        }

        CardFacade cardFacade = CreateCardView(handCard, mode);

        Debug.Log($"[CardSpawner] 為已存在卡牌創建視圖: {handCard.CardType} (ID: {handCard.CardId})");
        return cardFacade;
    }
    #endregion

    #region 輔助方法
    private CardFacade CreateCardView(HandCard card, GameMode mode)
    {
        // 使用 Factory 創建 CardFacade（包含 View 和 Movement）
        ICard cardInterface = _facadeFactory.Create(card, mode);
        CardFacade cardFacade = cardInterface as CardFacade;

        if (cardFacade == null)
        {
            Debug.LogError($"Factory 創建卡牌失敗: {card.CardType}");
            return null;
        }

        Debug.Log($"[CardSpawner] 生成卡牌: {card.CardType} (ID: {card.CardId})");
        return cardFacade;
    }
    #endregion
}
