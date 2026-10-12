using ChessPieceCardProduction;
using UnityEngine;
using Zenject;

//綁定UI、隨機事件、資源容器等輔助玩法
public class MainGameFieldInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        #region Game Flow 
        Container.BindInterfacesAndSelfTo<GameFlowStateMachine>().AsSingle();
        Container.Bind<IFlowFacade>().To<NullFlowFacade>().AsSingle();
        Container.Bind<GamePhaseModel>().AsSingle();
        Container.Bind<GamePhaseFacade>().AsSingle();
        Container.Bind<GamePhaseStateMachine>().FromComponentInHierarchy().AsSingle();

        Container.BindInterfacesAndSelfTo<PlayerProvider>().AsSingle();
        Container.BindInterfacesAndSelfTo<AIProvider>().AsSingle();

        Container.BindInterfacesAndSelfTo<PlayerMainSecPhaseStrategy>().AsSingle();
        Container.BindInterfacesAndSelfTo<AIMainSecPhaseStrategy>().AsSingle();

        Container.Bind<IGameFlowStateFactory>()
         .To<MainBattleStateFactory>()
         .AsSingle();

        Container.Bind<IVictoryCheckService>().To<VictoryCheckService>().AsSingle();
        Container.Bind<ITargetPieceTracker>().To<TargetFlagTrackService>().AsSingle();
        #endregion

        #region Camera Control
        Container.Bind<ICameraPresenter>().To<CameraPresenter>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<CameraInputService>().AsSingle();
        #endregion

        #region UI Elements
        // 主遊戲介面綁定
        Container.Bind<OptionPanelHandler>().FromComponentInHierarchy().AsSingle();
        Container.Bind<SettingButtonView>().FromComponentInHierarchy().AsSingle();

        //Container.Bind<IPieceInfoView>().To<PieceInfoView>().FromComponentInHierarchy().AsSingle();
        //Container.Bind<PieceInfoPresenter>().FromComponentInHierarchy().AsSingle();

        Container.Bind<GameFlowPresenter>().FromComponentInHierarchy().AsSingle();

        Container.BindInterfacesAndSelfTo<QuestTrackerView>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<QuestTrackerPresenter>().AsSingle();

        Container.Bind<GuidePanelView>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<GuidePanelPresenter>().AsSingle();

        // 資源容器介面綁定
        BindResourceContainer<SupplyContainer>("SupplyContainer");
        BindResourceContainer<MoraleContainer>("MoraleContainer");
        BindResourceContainer<PopularityContainer>("PopularityContainer");
        #endregion

        #region Signal Bindings
        // Game Flow Signal
        Container.DeclareSignal<GameStartedSignal>().OptionalSubscriber();
        Container.DeclareSignal<TurnChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<TurnEndedSignal>().OptionalSubscriber();
        Container.DeclareSignal<PhaseChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<PhaseEndRequestedSignal>().OptionalSubscriber();
        Container.DeclareSignal<ResolutionTriggeredSignal>().OptionalSubscriber();

        // Piece Signal
        Container.DeclareSignal<PreviewMoveSignal>().OptionalSubscriber();
        Container.DeclareSignal<PiecesSwappedSignal>().OptionalSubscriber();
        Container.DeclareSignal<BatchConfirmMovesSignal>().OptionalSubscriber();
        Container.DeclareSignal<MovesConfirmedSignal>().OptionalSubscriber();
        Container.DeclareSignal<MoveModeChangedSignal>().OptionalSubscriber();

        // Signal
        Container.DeclareSignal<CameraOpeningFinishedSignal>().OptionalSubscriber();
        Container.DeclareSignal<CameraModeChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<EnemyVisualMoveCompletedSignal>().OptionalSubscriber();
        Container.DeclareSignal<MarkTargetPieceSignal>().OptionalSubscriber();
        Container.DeclareSignal<ToggleHandDeckSignal>().OptionalSubscriber();
        Container.DeclareSignal<ChangeHandUIVisibilitySignal>().OptionalSubscriber();
        #endregion
    }

    #region SubContainer Binding (for Resource Containers)
    private void BindResourceContainer<T>(string prefabName) where T : ResourceContainerBase
    {
        Container.BindInterfacesAndSelfTo<T>()
                 .FromSubContainerResolve()
                 .ByNewPrefabResourceMethod(prefabName, InstallContainer)
                 .AsSingle();
    }
    private void InstallContainer(DiContainer container)
    {

    }
    #endregion
}