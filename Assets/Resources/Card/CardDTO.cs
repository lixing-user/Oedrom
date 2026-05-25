using ChessPieceCardProduction;
using Unity.InferenceEngine.Tokenization.PostProcessors.Templating;
using UnityEngine;
using Zenject;

namespace ChessPieceCardProduction
{
    // Infrastructure/Persistence/DTOs/HandCardDTO
    public class CardDTO 
    {
        public string cardId;
        public string pieceId;
        public ChessPieceType pieceType;
        public int team;
        public int baseCost;
        public int costModifier;
        public CardStatus status;

        public CardDTO() { }

        public CardDTO(HandCard card)
        {
            cardId = card.CardId.Value;
            pieceId = card.SourcePieceId?.Value;
            pieceType = card.CardType;
            team = (int)card.Team;
            baseCost = card.GetBaseCost();  // 需要公開 getter
            costModifier = card.GetCostModifier();  // 需要公開 getter
            status = card.GetStatus();
        }


        // 轉換為 Domain Entity
        public HandCard ToDomain()
        {
            var cardID = CardID.FromString(cardId);
            var pieceID = string.IsNullOrEmpty(pieceId) ? null : PieceID.FromString(pieceId);

            var card = new HandCard(
                cardID,
                pieceType,
                (Team)team,
                baseCost,
                pieceID
            );

            // 恢復運行時狀態
            if (costModifier != 0)
                card.ModifyCost(costModifier);
            if (status == CardStatus.broken)
                card.SetBroken();

            return card;
        }
    }
}


