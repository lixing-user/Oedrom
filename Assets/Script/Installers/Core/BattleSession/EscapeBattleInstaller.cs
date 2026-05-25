using UnityEngine;
using Zenject;

public class EscapeBattleInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        #region Game Flow 
        Container.BindInterfacesAndSelfTo<GameFlowStateMachine>().AsSingle();
        Container.Bind<IFlowFacade>().To<EBattleSessionFlowFacade>().AsSingle();

        Container.Bind<IGameFlowStateFactory>().To<EscapeBattleFlowStateFactory>().AsSingle();
        #endregion

        Container.BindInterfacesAndSelfTo<TimerService>().AsSingle();
        Container.Bind<TimerInterruptSupervisor>().AsSingle();

        Container.DeclareSignal<TimerStartedSignal>();
        Container.DeclareSignal<TimerTickSignal>();
        Container.DeclareSignal<TimerExpiredSignal>();

        Container.DeclareSignal<DeploymentConfirmedSignal>().OptionalSubscriber();

        #region Null Object
        Container.Bind<ICardInputManager>().To<NullCardInput>().AsSingle();

        Container.DeclareSignal<PiecesSwappedSignal>();
        Container.DeclareSignal<MoveModeChangedSignal>();
        #endregion
    }
}