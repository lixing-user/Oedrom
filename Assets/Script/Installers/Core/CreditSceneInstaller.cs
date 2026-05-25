using UnityEngine;
using Zenject;

public class CreditSceneInstaller : MonoInstaller
{
    [SerializeField] private TextAsset _creditsTextAsset;

    public override void InstallBindings()
    {
        // 綁定 Model (Zenject 會自動把 _creditsTextAsset 傳進 CreditModel 的建構子)
        Container.Bind<CreditModel>().AsSingle().WithArguments(_creditsTextAsset);

        Container.Bind<CreditScenePresenter>().FromComponentInHierarchy().AsSingle();
        Container.Bind<CreditSceneView>().FromComponentInHierarchy().AsSingle();
    }
}