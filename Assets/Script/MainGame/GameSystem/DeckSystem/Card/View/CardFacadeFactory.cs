using Systems.SceneManagement;
using UnityEngine;
using Zenject;

namespace ChessPieceCardProduction
{
    public interface ICardFactory : IFactory<HandCard, GameMode, ICard> { }

    public class CardFacadeFactory : PlaceholderFactory<HandCard, GameMode, ICard>, IValidatable, ICardFactory
    {
        private readonly GameModeStateMachine _gameMode;
        private readonly DiContainer _container;
        private readonly ICardConfigProvider _configProvider;
        private readonly Transform _handContainer;


        public CardFacadeFactory(
        DiContainer container,
        ICardConfigProvider configProvider,
        [Inject(Id = "DeckTransform")] Transform deckTransform)
        {
            _container = container;
            _configProvider = configProvider;
            _handContainer = deckTransform;
        }


        #region Public Methods

        public override ICard Create(HandCard handCard, GameMode gameMode)
        {
            #region 生成邏輯（新版）
            if (!_configProvider.TryGetConfig(handCard.CardType, out var cardConfig))
            {
                Debug.LogError($"找不到類型 {handCard.CardType} 的卡牌配置");
                return null;
            }

            // 2.  根據 GameMode 選擇 Prefab
            GameObject prefab = gameMode switch
            {
                GameMode.Mode2D => cardConfig.prefab2D,
                GameMode.Mode3D => cardConfig.prefab3D,
                _ => cardConfig.prefab3D
            };

            if (prefab == null)
            {
                Debug.LogError($"CardConfig '{cardConfig.name}' 缺少 {_gameMode.CurrentMode} 模式的 Prefab");
                return null;
            }

            //// 3.  實例化並注入 HandCard
            //var cardView = _container.InstantiatePrefabForComponent<CardFacade>(
            //    prefab,
            //    _handContainer
            //);


            //return cardView;

            if (prefab == null)
            {
                Debug.LogError($"找不到 {handCard.CardType} 的 Prefab");
                return null;
            }

            // 只實例化 CardFacade，並傳入參數
            var cardFacade = _container.InstantiatePrefabForComponent<CardFacade>(
                prefab,
                _handContainer
            );

            cardFacade.ManualInitialize(handCard, gameMode);

            Debug.Log($"[CardFacadeFactory] 生成卡牌: {handCard.CardType}");

            return cardFacade;
            #endregion
        }

        public override void Validate()
        {
            //Debug.Log("Validate Factory");
            //var testHandCard = new HandCard(ChessPieceType.Pawn, 0);
            //pawnFactory.Create(testHandCard, GameMode.Mode3D);
        }
        #endregion
    }
}
