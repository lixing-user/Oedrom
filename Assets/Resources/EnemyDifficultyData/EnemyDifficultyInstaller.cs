using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "EnemyDifficultyInstaller", menuName = "Installers/EnemyDifficultyInstaller")]
public class EnemyDifficultyInstaller : ScriptableObjectInstaller<EnemyDifficultyInstaller>
{
    [SerializeField]
    private EnemyDifficultyConfig difficultyConfig;


    public override void InstallBindings()
    {
        // 綁定靜態配置（提供給 Service 使用）
        Container.BindInstance(difficultyConfig).AsSingle();

        // 綁定動態難度計算服務（提供給生成系統或遊戲流程狀態機使用）
        //Container.Bind<IEnemyDifficultyService>().To<EnemyDifficultyService>().AsSingle();
    }
}