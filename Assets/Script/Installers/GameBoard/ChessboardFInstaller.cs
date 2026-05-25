using UnityEngine;
using Zenject;

public class ChessboardFInstaller : MonoInstaller
{
    [SerializeField] private ChessBoardFacade _boardFacade;
    [SerializeField] private StandbyZoneView _standbyZoneView;

    public override void InstallBindings()
    {
       Container.BindInterfacesAndSelfTo<ChessBoardFacade>().
       FromComponentInHierarchy().
       AsSingle();

        #region AI System
        Container.Bind<FairyStockfishManager>()
       .AsSingle()
       .WithArguments("my8*8Chess") // 場景專屬 variant
       .NonLazy();

        Container.Bind<IAIService>().To<AIService>().AsSingle();
        Container.Bind<IFallbackAIService>().To<FallbackTacticalAI>().AsSingle();

        Container.Bind<IBoardStateSnapshot>().To<BoardStateSnapshotProvider>().AsSingle();
        Container.Bind<FENAdapter>().AsSingle();
        #endregion

        #region Board Domain Service
        Container.Bind<Transform>()
          .WithId("BoardTransform")
          .FromInstance(_boardFacade.transform)
          .AsSingle();

        // 1. 先把兩種具體的 Handler 註冊進容器，供 DynamicCaptureHandler 內部解析使用
        Container.Bind<RemovalCaptureHandler>().AsSingle();
        Container.Bind<ShogiCaptureHandler>().AsSingle();

        // 2. 將 ICaptureHandler 綁定為動態的代理 Handler
        Container.Bind<ICaptureHandler>().To<DynamicCaptureHandler>().AsSingle();

        Container.Bind<IStandbyZone>().To<StandbyZone>().AsSingle();
        Container.BindInterfacesAndSelfTo<StandbyZoneService>().AsSingle();
        Container.Bind<StandbyLayoutCalculator>().AsSingle();
        Container.Bind<StandbyZoneView>().FromInstance(_standbyZoneView).AsSingle();
        #endregion

        #region Board Initialize
        Container.Bind<IBoardInitializationService>().To<BoardInitializeServiceF>().AsSingle();
        #endregion

        #region  Board Input & Piece Movement System
        // Input Piece Selection
        Container.Bind<IPieceSelectionExtension>()
        .To<BoardPieceSelectionExtension>()
        .AsSingle();

        Container.Bind<IPieceSelectionExtension>()
        .To<StandbyPieceSelectionExtension>()
        .AsSingle();

        Container.Bind<IPieceSelectionExtensionRegistry>()
        .To<PieceSelectionExtensionRegistry>()
        .AsSingle();

        Container.Bind<ICardPlacementRule>().To<DynamicPhasePlacementRule>().AsSingle();


        // Piece Move Strategy
        Container.Bind<DirectMoveStrategy>().AsSingle();
        Container.Bind<SetupMoveStrategy>().AsSingle();

        Container.Bind<IMoveExecutionStrategyFactory>()
        .To<MoveExecutionStrategyFactory>()
        .AsSingle();

        Container.Bind<IMoveModeService>()
        .To<MoveModeService>()
        .AsSingle()
        .WithArguments(MoveExecutionMode.Direct) // 場景默認模式
        .NonLazy();
        #endregion
    }
}