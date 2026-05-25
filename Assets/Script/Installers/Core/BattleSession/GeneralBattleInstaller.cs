using UnityEngine;
using Zenject;

public class GeneralBattleInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        #region Camera Control
        Container.BindInterfacesAndSelfTo<CameraPresenterS>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<CameraInputService>().AsSingle();
        #endregion

        #region UI Elements
        // 主遊戲介面綁定
        Container.Bind<OptionPanelHandler>().FromComponentInHierarchy().AsSingle();
        Container.Bind<SettingButtonView>().FromComponentInHierarchy().AsSingle();

        Container.Bind<GuidePanelView>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<GuidePanelPresenter>().AsSingle();

        // 資源容器介面綁定
        BindResourceContainer<SupplyContainer>("SupplyContainer");
        BindResourceContainer<MoraleContainer>("MoraleContainer");
        BindResourceContainer<PopularityContainer>("PopularityContainer");
        #endregion

        #region Signal Bindings
        // Game Flow Signal
        Container.DeclareSignal<GameStartedSignal>().OptionalSubscriber();
        Container.DeclareSignal<TurnChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<TurnEndedSignal>().OptionalSubscriber();

        Container.DeclareSignal<CameraOpeningFinishedSignal>().OptionalSubscriber();
        Container.DeclareSignal<CameraModeChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<MarkTargetPieceSignal>().OptionalSubscriber();
        Container.DeclareSignal<EnemyVisualMoveCompletedSignal>().OptionalSubscriber();
        Container.DeclareSignal<ToggleHandDeckSignal>().OptionalSubscriber();

        //null
        Container.DeclareSignal<PhaseEndRequestedSignal>().OptionalSubscriber();
        #endregion
    }

    #region SubContainer Binding (for Resource Containers)
    private void BindResourceContainer<T>(string prefabName) where T : ResourceContainerBase
    {
        Container.BindInterfacesAndSelfTo<T>()
                 .FromSubContainerResolve()
                 .ByNewPrefabResourceMethod(prefabName, InstallContainer)
                 .AsSingle();
    }
    private void InstallContainer(DiContainer container)
    {

    }
    #endregion
}