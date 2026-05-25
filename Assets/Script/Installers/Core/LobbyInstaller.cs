using UnityEngine;
using UnityEngine.Audio;
using Zenject;

public class LobbyInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<OptionPanelHandler>().FromComponentInHierarchy().AsSingle();
        Container.Bind<SettingButtonView>().FromComponentInHierarchy().AsSingle();
    }
}