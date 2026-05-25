using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "EventPoolInstaller", menuName = "Installers/EventPoolInstaller")]
public class EventPoolInstaller : ScriptableObjectInstaller<EventPoolInstaller>
{
    [SerializeField] private EventPoolConfig eventPoolConfig;

    public override void InstallBindings()
    {
        Container.BindInstance(eventPoolConfig).AsSingle();

        Container.BindInterfacesAndSelfTo<RandomEventManager>().AsSingle();
        Container.Bind<EventEffectDispatcher>().AsSingle();
        Container.Bind<EventEffectFactory>().AsSingle();
        Container.Bind<EffectContextFactory>().AsSingle();

    }
}