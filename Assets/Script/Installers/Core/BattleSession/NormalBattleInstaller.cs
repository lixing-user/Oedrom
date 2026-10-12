using UnityEngine;
using Zenject;

public class NormalBattleInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        #region Game Flow 
        Container.Bind<GameFlowModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<GameFlowStateMachine>().AsSingle();
        Container.Bind<IFlowFacade>().To<BattleSessionFlowFacade>().AsSingle();

        Container.Bind<IGameFlowStateFactory>()
         .To<SimplifiedBattleStateFactory>()
         .AsSingle();
        #endregion

        #region Board Inplementation
        Container.Bind<IPieceDeploymentService>().To<PieceDeploymentService>().AsSingle();
        Container.BindInterfacesAndSelfTo<DeploymentValidator>().AsSingle();

        Container.Bind<IRewardService>().To<BattleRewardService>().AsSingle();
        Container.Bind<IResourceRewardStrategy>().To < DifficultyBasedRewardStrategy >().AsSingle();

        Container.BindInterfacesAndSelfTo<MoveCounterService>().AsSingle();

        Container.BindInterfacesAndSelfTo<SetupFogService>().AsSingle();
        Container.Bind<IFogRenderer>().To<FogRenderer>().AsSingle();
        #endregion

        #region Signal Bindings
        // Piece Signal
        Container.DeclareSignal<PiecesSwappedSignal>().OptionalSubscriber();
        Container.DeclareSignal<PieceRestoredSignal>().OptionalSubscriber();

        Container.DeclareSignal<MoveModeChangedSignal>().OptionalSubscriber();

        // Game Service Signal
        Container.DeclareSignal<ArrangePieceRequestedSignal>().OptionalSubscriber();
        Container.DeclareSignal<CleanUpPieceRequestedSignal>().OptionalSubscriber();
        Container.DeclareSignal<PiecesArrangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<PiecesClearedSignal>().OptionalSubscriber();
        Container.DeclareSignal<DeploymentConfirmedSignal>().OptionalSubscriber();
        Container.DeclareSignal<MoveCountChangedSignal>();
        Container.DeclareSignal<ShowRewardRequestedSignal>().OptionalSubscriber();
        Container.DeclareSignal<ShowChaseBattleRequestedSignal>().OptionalSubscriber();

        Container.DeclareSignal<ToggleHandDeckSignal>().OptionalSubscriber();
        Container.DeclareSignal<StandbyZoneUpdatedSignal>().OptionalSubscriber();

        #endregion
    }
}