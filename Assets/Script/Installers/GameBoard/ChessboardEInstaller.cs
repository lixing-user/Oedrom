using UnityEngine;
using Zenject;

public class ChessboardEInstaller : MonoInstaller
{
    [SerializeField] private ChessBoardFacade _boardFacade;
    [SerializeField] private GameObject _escapePointPrefab;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<ChessBoardFacade>()
        .FromComponentInHierarchy()
        .AsSingle();

        #region AI System
        Container.Bind<IAIService>().To<ManhattanChaserAIService>().AsSingle();
        Container.Bind<IBoardStateSnapshot>().To<BoardStateSnapshotProvider>().AsSingle();
        Container.Bind<FENAdapter>().AsSingle();
        #endregion

        #region Board Domain Service
        Container.Bind<Transform>()
         .WithId("BoardTransform")
         .FromInstance(_boardFacade.transform)
         .AsSingle();

        Container.Bind<ICaptureHandler>().To<RemovalCaptureHandler>().AsSingle();
        #endregion

        #region Board Initialize
        Container.Bind<IBoardInitializationService>().To<BoardInitializeServiceE>().AsSingle();

        Container.BindInterfacesAndSelfTo<BattleResetCoordinator>().AsSingle();
        Container.Bind<IBattleStateRestorer>().To<StaticBattleRestorer>().AsSingle();
        Container.Bind<IRewindVisualPresenter>().To<RewindVisualDirector>().AsSingle();
        Container.Bind<RewindTransitionView>().FromComponentInHierarchy().AsSingle();

        Container.Bind<IShardSpawner>().To<EscapePointSpawner>().AsSingle();
        Container.BindFactory<PiecePosition, Vector3, EscapePointView, EscapePointView.Factory>()
           .FromComponentInNewPrefab(_escapePointPrefab)
           .UnderTransformGroup("EscapePoint");
        #endregion

        #region  Board Input & Piece Movement System
        // Input Piece Selection
        Container.Bind<IPieceSelectionExtension>()
        .To<BoardPieceSelectionExtension>()
        .AsSingle();

        Container.Bind<IPieceSelectionExtensionRegistry>()
        .To<PieceSelectionExtensionRegistry>()
        .AsSingle();

        Container.Bind<ICardPlacementRule>().To<BottomRowPlacementRule>().AsSingle();

        // Piece Move Strategy
        Container.Bind<DirectMoveStrategy>().AsSingle();

        Container.Bind<IMoveExecutionStrategyFactory>()
        .To<MoveExecutionStrategyFactory>()
        .AsSingle();

        Container.Bind<IMoveModeService>()
        .To<MoveModeService>()
        .AsSingle()
        .WithArguments(MoveExecutionMode.Direct) // 놓뉴픮�{소─
        .NonLazy();
        #endregion
    }
}