using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "BoardSettingInstaller", menuName = "Installers/BoardSettingInstaller")]
public class BoardSettingInstaller : ScriptableObjectInstaller<BoardSettingInstaller>
{
    public BoardSettings BasicBoardSettings;

    public TerrainBoardSettings TerrainSettings;

    public TerrainHeightThreshold HeightThreshold;

    public override void InstallBindings()
    {
        Container.BindInstance(BasicBoardSettings).IfNotBound();

        if (TerrainSettings != null)
        {
            Container.BindInstance(TerrainSettings).IfNotBound();
            Container.BindInstance(HeightThreshold).IfNotBound();
        }
    }
}