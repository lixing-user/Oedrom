using ChessPieceCardProduction;
using Systems.SceneManagement;
using UnityEngine;
using Zenject;

public class ShopSceneInstaller : MonoInstaller
{
    [SerializeField] private HandCardContainer _handContainer;

    public override void InstallBindings()
    {
        Container.Bind<IDomainEventDispatcher>().To<DomainEventDispatcher>().AsSingle();

        #region Shop System
        Container.Bind<Shop>().AsSingle();

        Container.Bind<ShopFacade>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<ShopApplicationService>().AsSingle();

        Container.Bind<IShopFactory>().To<ShopFactory>().AsSingle();
        Container.Bind<ShopItemSpawner>().AsSingle();

        Container.Bind<ShopScenePresenter>().FromComponentInHierarchy().AsSingle();

        #endregion

        #region Handdeck System
        Container.Bind<Transform>()
        .WithId("DeckTransform")
        .FromInstance(_handContainer.handCardDeck.transform);

        Container.BindInterfacesAndSelfTo<ShopHandUIManager>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<HandCardContainer>().FromComponentInHierarchy().AsSingle();

        Container.Bind<ICardInputService>().To<ShopInputService>().AsSingle();
        Container.BindInterfacesAndSelfTo<ShopInputHandler>().AsSingle();
        Container.BindInterfacesAndSelfTo<ShopCardMoveVisualizer>().FromComponentInHierarchy().AsSingle();

        Container.BindInterfacesAndSelfTo<CardSpawner>().AsSingle();

        Container.BindFactoryCustomInterface<HandCard, GameMode, ICard, CardFacadeFactory, ICardFactory>()
            .FromFactory<CardFacadeFactory>();
        #endregion

        // 主遊戲介面綁定
        Container.Bind<OptionPanelHandler>().FromComponentInHierarchy().AsSingle();
        Container.Bind<SettingButtonView>().FromComponentInHierarchy().AsSingle();

        #region Signals Binding
        Container.DeclareSignal<ShopItemClickedSignal>().OptionalSubscriber();
        Container.DeclareSignal<RequestSectionChangeSignal>().OptionalSubscriber();
        Container.DeclareSignal<RequestLootBattleSignal>().OptionalSubscriber();
        Container.DeclareSignal<RequestAddSellItemSignal>().OptionalSubscriber();
        Container.DeclareSignal<RequestAddRepairItemSignal>().OptionalSubscriber();

        Container.DeclareSignal<ToggleHandDeckSignal>().OptionalSubscriber();
        #endregion
    }
}