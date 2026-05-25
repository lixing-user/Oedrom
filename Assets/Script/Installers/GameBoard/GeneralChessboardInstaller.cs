using UnityEngine;
using Zenject;

public class GeneralChessboardInstaller : MonoInstaller
{
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private GameObject _targetFlagPrefab;

    public override void InstallBindings()
    {
        Container.BindInstance(BoardContextId.BattleBoard).AsSingle();

        #region Board Domain Service
        Container.Bind<BoardDimensionsFactory>().AsSingle();
        Container.Bind<BoardDimensions>()
           .FromMethod(ctx =>
           {
               var factory = ctx.Container.Resolve<BoardDimensionsFactory>();
               return factory.Create();
           })
           .AsSingle();
        Container.BindInterfacesAndSelfTo<GameBoard<Tile>>().AsSingle();

        Container.Bind<IDomainEventDispatcher>().To<DomainEventDispatcher>().AsSingle();

        Container.Bind<IBoardQueryService>().To<BoardQueryService>().AsSingle();

        Container.Bind<IPieceFactory>().To<PieceFactory>().AsSingle();

        // Domain Event System
        Container.BindInterfacesAndSelfTo<PieceEventHandler>().AsSingle().NonLazy();
        #endregion

        #region Board Initialize
        Container.Bind<ICoordinateConverter>().To<CoordinateConverter>().AsSingle();
        Container.Bind<ITerrainHeightSampler>().To<TerrainHeightSampler>().AsSingle();


        // Tile Generate
        Container.BindInterfacesAndSelfTo<BoardTileGenerator>().AsSingle();
        Container.BindInterfacesAndSelfTo<TilePositionCalculator>().AsSingle();
        Container.BindInterfacesAndSelfTo<FlatTileHeightCalculator>().AsSingle();

        Container.BindFactory<TilePosition, Vector3, TileView, TileView.Factory>()
          .FromComponentInNewPrefab(_tilePrefab)
          .UnderTransformGroup("Tiles");


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


        // Piece Move Execute
        Container.Bind<IMovementStrategyFactory>().To<MovementStrategyFactory>().AsSingle();
        Container.Bind<IMoveHandler>().To<ChessMoveHandler>().AsSingle();


        // Piece Move Validate
        Container.Bind<IMoveValidateService>().To<MoveValidator>().AsSingle();


        //Piece Visualize
        Container.BindInterfacesAndSelfTo<BoardVisualizer>().AsSingle();
        Container.Bind<IPiecePreviewVisualizerService>().To<PiecePreviewVisualizerService>()
        .FromComponentInHierarchy()
        .AsSingle()
        .NonLazy();

        Container.Bind<IArrowVisualizerPresenter>().To<ArrowVisualizerPresenter>().FromComponentInHierarchy().AsSingle();
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

        #endregion

        #region Tutorial System
        Container.BindInterfacesAndSelfTo<BattleResultGatekeeper>().AsSingle();
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