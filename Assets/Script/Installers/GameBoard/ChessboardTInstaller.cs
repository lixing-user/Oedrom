using System;
using Unity.InferenceEngine;
using UnityEngine;
using Zenject;

//Chessboard for Terrain 完成SRP的棋盤核心玩法綁定 == 原本的 Chessboard for Terrain
public class ChessboardTInstaller : MonoInstaller
{
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private GameObject _landmarkPrefab;
    [SerializeField] private GameObject _targetFlagPrefab;
    [SerializeField] private GameObject _tilePointerPrefab;
    [SerializeField] private ChessBoardTFacade _boardFacade;

    public override void InstallBindings()
    {
        Container.BindInstance(BoardContextId.MainGameField).AsSingle();

        Container.BindInterfacesAndSelfTo<ChessBoardTFacade>().
            FromComponentInHierarchy().
            AsSingle();

        #region AI System
        Container.Bind<FairyStockfishManager>()
           .AsSingle()
           .WithArguments("SandboxChess") // 場景專屬 variant
           .NonLazy();

        Container.Bind<IAIService>().To<AIService>().AsSingle();
        Container.Bind<IFallbackAIService>().To<MGFFallbackAI>().AsSingle();
        Container.Bind<IBoardStateSnapshot>().To<BoardStateSnapshotProvider>().AsSingle();
        Container.Bind<FENAdapter>().AsSingle();
        #endregion

        #region Board Domain Service
        // Domain and Repository
        //轉換infrastructure成domain需要的BoardDimensions
        Container.Bind<BoardDimensionsFactory>().AsSingle();
        Container.Bind<BoardDimensions>()
           .FromMethod(ctx =>
           {
               var factory = ctx.Container.Resolve<BoardDimensionsFactory>();
               return factory.Create();
           })
           .AsSingle();

        Container.BindInterfacesAndSelfTo<GameBoard<TerrainTile>>().AsSingle();

        Container.Bind<IBoardQueryService>().To<BoardQueryServiceT>().AsSingle();

        Container.Bind<IPieceFactory>().To<PieceFactory>().AsSingle();

        Container.Bind<Transform>()
            .WithId("BoardTransform")
            .FromInstance(_boardFacade.transform)
            .AsSingle();

        Container.Bind<ITerrainRepository>().To<TerrainRepository>().AsSingle();

        Container.Bind<ITerrainTypeClassifier>().To<TerrainTypeClassifier>().AsSingle();

        Container.Bind<ICaptureHandler>().To <RemovalCaptureHandler>().AsSingle();

        // Domain Event System
        Container.BindInterfacesAndSelfTo<PieceEventHandler>().AsSingle().NonLazy();
        #endregion

        #region Board Initialize
        Container.Bind<IBoardInitializationServiceT>().To<BoardInitializeServiceT>().AsSingle();
        Container.Bind<ICoordinateConverter>().To<CoordinateConverter>().AsSingle();
        Container.Bind<ITerrainHeightSampler>().To<TerrainHeightSampler>().AsSingle();

        // Tile Generate
        Container.BindInterfacesAndSelfTo<BoardTileGenerator>().AsSingle();
        Container.BindInterfacesAndSelfTo<TilePositionCalculator>().AsSingle();
        Container.Bind<ITileHeightCalculator>().To<TileHeightCalculator>().AsSingle();
        Container.BindInterfacesAndSelfTo<BoardTerrainHeightAdapter>().AsSingle();

        Container.BindFactory<TilePosition, Vector3, TileView, TileView.Factory>()
           .FromComponentInNewPrefab(_tilePrefab)
           .UnderTransformGroup("Tiles");

        // Landmark Generate
        Container.BindInterfacesAndSelfTo<LandmarkSpawner>().AsSingle();
        Container.BindFactory<LandmarkType, PiecePosition, Vector3, LandmarkView, LandmarkView.Factory>()
            .FromComponentInNewPrefab(_landmarkPrefab)
            .UnderTransformGroup("Landmarks");
        Container.Bind<ILandmarkViewRegistry>()
         .To<LandmarkViewRegistry>()
         .AsSingle();

        // Piece Generate
        Container.BindInterfacesAndSelfTo<PieceSpawner>().AsSingle();

        Container.BindFactory<ChessPieceType, ChessPiece, Vector3, PieceView, PieceView.Factory>()
             .FromSubContainerResolve()
             .ByMethod(InstallPieceViewSubContainer)  // 關鍵：子容器安裝方法
             .AsTransient();
        #endregion

        #region  Board Input & Piece Movement System
        // Input Handling
        Container.BindInterfacesAndSelfTo<BoardInputHandler>().AsSingle();
        Container.Bind<IBoardInputService>().To<BoardInputService>().AsSingle();

        Container.Bind<IPieceSelectionExtensionRegistry>()
        .To<PieceSelectionExtensionRegistry>()
        .AsSingle();

        Container.Bind<IPieceSelectionExtension>()
        .To<BoardPieceSelectionExtension>()
        .AsSingle();

        Container.Bind<ICardPlacementRule>().To<BottomRowPlacementRule>().AsSingle();
        Container.Bind<ICardSelectionExtension>().To<TutorialCardSelectionExtension>().AsSingle();

        // Piece Movement
        Container.Bind<IMovementStrategyFactory>().To<MovementStrategyFactory>().AsSingle();

        //Container.Bind<IMoveExecutionStrategy>().To<PreviewMoveStrategy>().AsSingle();
        Container.Bind<PreviewMoveStrategy>().AsSingle();

        Container.Bind<IMoveExecutionStrategyFactory>()
        .To<MoveExecutionStrategyFactory>()
        .AsSingle();

        Container.Bind<IMoveModeService>()
        .To<MoveModeService>()
        .AsSingle()
        .WithArguments(MoveExecutionMode.Preview) // 這個場景默認預覽模式
        .NonLazy();

        Container.Bind<IMoveHandler>().To<ChessMoveHandler>().AsSingle();
        Container.BindInterfacesTo<BatchConfirmMovesHandler>().AsSingle();
        //Container.Bind<TileOccupancyManager>().AsSingle();

        // Piece Move Validate
        Container.Bind<IMoveValidateService>().To<MoveValidatorT>().AsSingle();
        Container.Bind<ITerrainMoveRule>().To<TerrainMoveRule>().AsSingle();

        //Piece Visualize
        Container.BindInterfacesAndSelfTo<BoardVisualizer>().AsSingle();
        Container.Bind<IPiecePreviewVisualizerService>().To<PiecePreviewVisualizerService>()
            .FromComponentInHierarchy()
            .AsSingle()
            .NonLazy();

        Container.Bind<IArrowVisualizerPresenter>().To<ArrowVisualizerPresenter>()
            .FromComponentInHierarchy()
            .AsSingle();
        Container.Bind<IArrowVisualizerService>().To<ArrowVisualizerService>().AsSingle();

        Container.Bind<IPieceViewRegistry>()
            .To<PieceViewRegistry>()
            .AsSingle();

        Container.BindInterfacesAndSelfTo<PieceVisualPresenter>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<TargetIndicatorPresenter>().AsSingle().NonLazy();

        Container.BindMemoryPool<TargetFlagView, TargetFlagView.Pool>()
             .WithInitialSize(2)
             .FromComponentInNewPrefab(_targetFlagPrefab)
             .UnderTransformGroup("VisualIndicators");


        Container.BindMemoryPool<TilePointerView, TilePointerView.Pool>()
        .WithInitialSize(1)
        .FromComponentInNewPrefab(_tilePointerPrefab)
        .UnderTransformGroup("VisualIndicators"); 

        Container.Bind<ITilePointerService>().To<TilePointerService>().AsSingle();
        #endregion
    }

    #region SubContainer Binding (for Piece View)
    private void InstallPieceViewSubContainer(
        DiContainer container, 
        ChessPieceType type, 
        ChessPiece piece, 
        Vector3 worldPos)
    {
        container.BindInstance(type).WhenInjectedInto<PieceView>();
        container.BindInstance(piece).WhenInjectedInto<PieceView>();
        container.BindInstance(worldPos).WithId("InitialPosition");

        var settings = container.Resolve<BoardSettings>();
        container.BindInstance(settings);

        GameObject prefab = GetPrefabByType(type, settings);
        container.Bind<PieceView>()
                    .FromComponentInNewPrefab(prefab)
                    .UnderTransformGroup("Pieces")
                    .AsSingle()
                    .NonLazy();
    }

    private GameObject GetPrefabByType(ChessPieceType type, BoardSettings _settings)
    {
        return type switch
        {
            ChessPieceType.Pawn => _settings.ChessPiecePrefabs[0],
            ChessPieceType.Rook => _settings.ChessPiecePrefabs[1],
            ChessPieceType.Knight => _settings.ChessPiecePrefabs[2],
            ChessPieceType.Bishop => _settings.ChessPiecePrefabs[3],
            ChessPieceType.Queen => _settings.ChessPiecePrefabs[4],
            ChessPieceType.King => _settings.ChessPiecePrefabs[5],
            _ => null
        };
    }
    #endregion
}