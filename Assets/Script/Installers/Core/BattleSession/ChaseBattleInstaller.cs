using UnityEngine;
using Zenject;

public class ChaseBattleInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        #region Game Flow 
        Container.Bind<GameFlowModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<GameFlowStateMachine>().AsSingle();
        Container.Bind<IFlowFacade>().To<CBattleSessionFlowFacade>().AsSingle();

        Container.Bind<IGameFlowStateFactory>().To<ChaseBattleFlowStateFactory>().AsSingle();
        #endregion

        #region Board Inplementation
        Container.Bind<IRewardService>().To<BattleRewardService>().AsSingle();
        Container.Bind<IResourceRewardStrategy>().To<RandomRewardStrategy>().AsSingle();

        Container.BindInterfacesAndSelfTo<MoveCounterService>().AsSingle();
        #endregion

        Container.DeclareSignal<MoveCountChangedSignal>();
       
        #region Null Object
        Container.Bind<ICardInputManager>().To<NullCardInput>().AsSingle();

        Container.DeclareSignal<PiecesSwappedSignal>();
        Container.DeclareSignal<MoveModeChangedSignal>();
        #endregion
    }
}