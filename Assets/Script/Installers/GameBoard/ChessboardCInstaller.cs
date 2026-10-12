using UnityEngine;
using Zenject;

public class ChessboardCInstaller : MonoInstaller
{
    [SerializeField] private ChessBoardFacade _boardFacade;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<ChessBoardFacade>().
        FromComponentInHierarchy().
        AsSingle();

        #region Board Initialize
        Container.Bind<IBoardInitializationService>().To<BoardInitializeServiceC>().AsSingle();

        //Container.BindInterfacesAndSelfTo<BattleResetCoordinator>().AsSingle();
        //Container.Bind<IBattleStateRestorer>().To<StaticBattleRestorer>().AsSingle();
        //Container.Bind<IRewindVisualPresenter>().To<RewindVisualDirector>().AsSingle();
        //Container.Bind<RewindTransitionView>().FromComponentInHierarchy().AsSingle();
        #endregion

        #region AI System
        Container.Bind<FairyStockfishManager>()
       .AsSingle()
       .WithArguments("chess")  // 呼叫 Fairy-Stockfish 內建的標準西洋棋規則
       .NonLazy();

        Container.Bind<IAIService>().To<AIService>().AsSingle();
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

        #region  Board Input & Piece Movement System
        // Input Piece Selection
        Container.Bind<IPieceSelectionExtension>()
        .To<BoardPieceSelectionExtension>()
        .AsSingle();

        Container.Bind<IPieceSelectionExtensionRegistry>()
        .To<PieceSelectionExtensionRegistry>()
        .AsSingle();

        // Piece Move Strategy
        Container.Bind<DirectMoveStrategy>().AsSingle();

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