using UnityEngine;
using Zenject;

public class StorySceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IStoryView>().To<StoryLayoutView>().FromComponentInHierarchy().AsSingle();
        Container.Bind<IStoryPresenter>().To<StoryLayoutPresenter>().FromComponentInHierarchy().AsSingle();

        Container.BindInterfacesAndSelfTo<DialogueService>().AsSingle();
        Container.BindInterfacesAndSelfTo<StorySpriteService>().AsSingle();

        Container.Bind<StoryStateMachine>().FromComponentInHierarchy().AsSingle();
        Container.Bind<StoryPhaseFacade>().FromComponentInHierarchy().AsSingle();

        #region Signals
        Container.DeclareSignal<DialogueEndedSignal>().OptionalSubscriber();
        #endregion
    }
}