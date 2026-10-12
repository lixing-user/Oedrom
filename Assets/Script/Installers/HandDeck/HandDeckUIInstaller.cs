using ChessPieceCardProduction;
using ModestTree;
using Systems.SceneManagement;
using UnityEngine;
using Zenject;

public class HandDeckUIInstaller : MonoInstaller
{
    [SerializeField] private HandCardContainer _handContainer;

    public override void InstallBindings()
    {
        Container.Bind<Transform>()
         .WithId("DeckTransform")
         .FromInstance(_handContainer.handCardDeck.transform);

        Container.BindInterfacesAndSelfTo<HandUIInputDetector>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<HandCardContainer>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<HandUIStateManager>().AsSingle();

        Container.BindInterfacesAndSelfTo<CardInputService>().AsSingle();
        Container.BindInterfacesAndSelfTo<CardInputHandler>().AsSingle();
        Container.BindInterfacesAndSelfTo<CardMoveVisualizer>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<CardPlacementHandler>().AsSingle();

        Container.BindInterfacesAndSelfTo<CardSpawner>().AsSingle();

        Container.BindFactoryCustomInterface<HandCard, GameMode, ICard, CardFacadeFactory, ICardFactory>()
            .FromFactory<CardFacadeFactory>();
    }
}