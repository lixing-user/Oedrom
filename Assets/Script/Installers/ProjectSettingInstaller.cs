using UnityEngine;
using Zenject;


[CreateAssetMenu(fileName = "ProjectSettingInstaller", menuName = "Installers/ProjectSettingInstaller")]
public class ProjectSettingInstaller : ScriptableObjectInstaller<ProjectSettingInstaller>
{
    public override void InstallBindings()
    {
        // You can bind project-wide settings or services here
        // For example:
        // Container.Bind<IProjectSettings>().To<ProjectSettings>().AsSingle();
    }
}