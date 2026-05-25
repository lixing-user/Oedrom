using ChessPieceCardProduction;
using UnityEngine;
using Zenject;

// Application/Hand/CardFactory.cs
public class CardDataFactory : ICardDataFactory
{
    private readonly CardStaticDataRegistry _staticDataRegistry;

    [Inject]
    public CardDataFactory(CardStaticDataRegistry staticDataRegistry)
    {
        _staticDataRegistry = staticDataRegistry;
    }


    #region 創建卡牌
    // 創建新的 HandCard（用於新遊戲或獲得新卡牌時）
    public HandCard CreateCard(ChessPieceType type, Team team)
    {
        int baseCost = _staticDataRegistry.GetBaseCost(type);
        var cardId = CardID.Generate();

        Debug.Log($"[CardDataFactory] 創建新卡牌: {type} (ID: {CardID.Generate()})");

        return new HandCard(
            cardId,
            type,
            team,
            baseCost
        );
    }

    // 創建新的 HandCard 並設定狀態
    public HandCard CreateCard(ChessPieceType type, Team team, CardStatus status)
    {
        var card = CreateCard(type, team);

        if (status == CardStatus.broken)
            card.SetBroken();

        return card;
    }

    // 從 ChessPiece 創建 HandCard（用於從棋子生成卡牌時）
    public HandCard CreateCardFromPiece(ChessPiece piece)
    {
        if (piece == null)
            throw new System.ArgumentNullException(nameof(piece));

        int baseCost = _staticDataRegistry.GetBaseCost(piece.Type);
        var cardId = CardID.Generate();

        return new HandCard(
            cardId,
            piece.Type,
            piece.Team,
            baseCost,
            piece.Id  // 記錄來源棋子
        );
    }
    #endregion

    #region  讀檔功能
    // 從 DTO 恢復 HandCard（用於讀檔）
    public HandCard CreateFromDTO(CardDTO dto)
    {
        PieceID sourcePieceId = !string.IsNullOrEmpty(dto.pieceId)
       ? PieceID.FromString(dto.pieceId)
       : null;

        var card = new HandCard(
            CardID.FromString(dto.cardId),

            dto.pieceType,
            (Team)dto.team,
            dto.baseCost,
            sourcePieceId
        );

        if (dto.costModifier != 0)
            card.ModifyCost(dto.costModifier);
        if (dto.status == CardStatus.broken)
            card.SetBroken();

        return card;
    }
    #endregion
}

public interface ICardDataFactory
{
    HandCard CreateCard(ChessPieceType type, Team team);
    HandCard CreateCard(ChessPieceType type, Team team, CardStatus status);
    HandCard CreateCardFromPiece(ChessPiece piece);
    HandCard CreateFromDTO(CardDTO dto);
}