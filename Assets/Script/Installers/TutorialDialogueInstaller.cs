using UnityEngine;
using Zenject;

public class TutorialDialogueInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        #region test 
        //Container.Rebind<SceneGameActionContext>().AsSingle();
        Container.Bind<SceneGameActionContext>().AsSingle();
        #endregion

        Container.Bind<TutorialStepRunner>().FromComponentInHierarchy().AsSingle();
        Container.Bind<TutorialTargetRegistry>().FromComponentInHierarchy().AsSingle();

        Container.Bind<IDialogueBoxManager>().To<DialogueBoxHandler>().AsSingle();
    }
}