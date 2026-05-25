using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "MarchingCubeSetting", menuName = "Installers/MarchingCubeSetting")]
public class MarchingCubeSettingInstaller : ScriptableObjectInstaller<MarchingCubeSettingInstaller>
{
    public MarchingCubeSetting _marchingCube;

    public override void InstallBindings()
    {
        Container.BindInstance(_marchingCube).IfNotBound();
    }
}