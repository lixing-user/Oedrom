using UnityEngine;
using Zenject;

public class ChessboardNInstaller : MonoInstaller
{
    [SerializeField] private ChessBoardFacade _boardFacade;
    [SerializeField] private StandbyZoneView _standbyZoneView;
    [SerializeField] private RewardCardSlot rewardSlotPrefab;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<ChessBoardFacade>().
        FromComponentInHierarchy().
        AsSingle();

        #region AI System
        Container.Bind<FairyStockfishManager>()
       .AsSingle()
       .WithArguments("my6x6chess") // 場景專屬 variant
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

        Container.Bind<IEnemyDifficultyService>().To<EnemyDifficultyService>().AsSingle();
        #endregion

        #region Board Initialize & Resolution
        Container.Bind<IBoardInitializationService>().To<BoardInitializeServiceN>().AsSingle();

        Container.BindInterfacesAndSelfTo<BattleResetCoordinator>().AsSingle();
        Container.BindInterfacesAndSelfTo<NormalBattleRestorer>().AsSingle();
        Container.Bind<IRewindVisualPresenter>().To<RewindVisualDirector>().AsSingle();
        Container.Bind<RewindTransitionView>().FromComponentInHierarchy().AsSingle();

        Container.BindMemoryPool<RewardCardSlot, RewardCardSlotPool>()
        .WithInitialSize(5) // 初始先做 5 個放著備用
        .FromComponentInNewPrefab(rewardSlotPrefab)
        .UnderTransformGroup("RewardSlotsPool");
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