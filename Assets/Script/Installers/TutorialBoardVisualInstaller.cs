using UnityEngine;
using Zenject;

public class TutorialBoardVisualInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<TutorialHighlightService>().FromComponentInHierarchy().AsSingle();
    }
}