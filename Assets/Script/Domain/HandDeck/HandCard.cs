using System;
using UnityEngine;

// Domain : entity 
namespace ChessPieceCardProduction
{
    public class HandCard
    {
        public CardID CardId { get; private set; }
        public PieceID SourcePieceId { get; private set; }

        public ChessPieceType CardType { get; }
        public Team Team { get; }

        private int _baseCost;
        private int _costModifier;
        private CardStatus _status;

        public HandCard(
            CardID id,
            ChessPieceType type, 
            Team team, 
            int baseCost,
            PieceID pieceID = null)
        {
            CardId = id;
            SourcePieceId = pieceID;
            CardType = type;
            Team = team;
            _baseCost = baseCost;
            _status = CardStatus.normal;
        }

        #region 業務邏輯
        public int GetBaseCost() { return _baseCost; }
        public int GetCostModifier() { return _costModifier; }
        public int GetTotalCost() => _baseCost + _costModifier;
        public CardStatus GetStatus() { return _status; }
        public bool IsFromPiece() => SourcePieceId != null;


        public int CalculateSellValue()
        {
            float ratio = (_status == CardStatus.broken) ? 0.2f : 0.5f;
            float value = GetTotalCost() * ratio;
            return (int)System.Math.Round(value); // 不依賴 Unity
        }

        public void ModifyCost(int modifier)
        {
            if (GetTotalCost() + modifier < 0)
                throw new InvalidOperationException("費用不能為負數");
            _costModifier += modifier;
        }

            #region 戰損機制
            public void SetBroken()
            {
                if (_status == CardStatus.broken)
                    throw new InvalidOperationException("卡牌已經是破損狀態");
                _status = CardStatus.broken;
            }

            public void Repair()
            {
                if (_status == CardStatus.normal)
                    throw new InvalidOperationException("卡牌已經是完好狀態");
                _status = CardStatus.normal;
            }
            #endregion
        #endregion
    }

    public enum CardStatus
    {
        broken,
        normal
    }
}
