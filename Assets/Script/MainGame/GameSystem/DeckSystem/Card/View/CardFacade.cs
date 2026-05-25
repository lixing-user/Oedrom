using ChessPieceCardProduction;
using Systems.SceneManagement;
using UnityEngine;
using Zenject;

namespace ChessPieceCardProduction
{
    // Presentation/Hand/CardFacade.cs
    public class CardFacade : MonoBehaviour, ICard
    {
        [Header("卡牌基本信息")]
        [SerializeField] private string _cardIdValue;
        [SerializeField] private string _sourcePieceIdValue;

        private HandCard _model;
        private GameMode _gameMode;

        private CardView _view;
        private CardInputInspector _movement;


        public void ManualInitialize(HandCard domainCard, GameMode mode)
        {
            _model = domainCard;
            _gameMode = mode;
            _cardIdValue = domainCard.CardId.ToString();
            _sourcePieceIdValue = domainCard.SourcePieceId != null
                ? domainCard.SourcePieceId.ToString()
                : "None (非棋子來源)";

            Debug.Log($"[CardFacade] ManualInitialize: {_model.CardType} (ID: {_model.CardId})");
            Debug.Log($"[CardFacade] Transform.parent: {transform.parent?.name ?? "NULL"}");  // 檢查父節點
            Debug.Log($"[CardFacade] GameObject.name: {gameObject.name}");  // 檢查物件名稱
            Debug.Log($"[CardFacade] Position: {transform.position}");  // 檢查位置
            // 如果已經 Awake 過，立即初始化 View
            if (_view != null)
            {
                _view.Initialize(_model);
            }
        }


        private void Awake()
        {
            _view = GetComponent<CardView>();
            _movement = GetComponent<CardInputInspector>();

            if (_model != null)
            {
                _cardIdValue = _model.CardId.ToString();
                _sourcePieceIdValue = _model.SourcePieceId != null
                    ? _model.SourcePieceId.ToString()
                    : "None (非棋子來源)";

                if (_view != null)
                {
                    _view.Initialize(_model);
                }

                Debug.Log($"[CardFacade] 初始化: {_model.CardType} (ID: {_model.CardId})");
            }
        }


        #region Public API
        public HandCard GetDomainCard() => _model;

        public void UpdateDisplay()
        {
            _view?.UpdateCardDisplay(_model);
        }

        public void SetInteractable(bool interactable)
        {
            if (_movement != null)
                _movement.enabled = interactable;
        }
        #endregion

        #region Domain 查詢（供其他系統使用）
        /// <summary>
        /// 獲取 Domain Model 的 ID（用於 Repository 查詢）
        /// </summary>
        public CardID GetModelId() => _model?.CardId;
        public PieceID GetSourcePieceId() => _model?.SourcePieceId;
        public bool IsFromPiece() => _model?.SourcePieceId != null;

        public ChessPieceType GetPieceType() => _model?.CardType ?? ChessPieceType.None;
        public Team GetTeam() => _model?.Team ?? Team.White;
        #endregion
    }

    public interface ICard { }
}