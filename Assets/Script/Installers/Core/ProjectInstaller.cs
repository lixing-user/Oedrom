using ChessPieceCardProduction;
using Systems.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [Header("遊戲設定參數")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("新手引導劇本")]
    //[SerializeField] private OnboardingFlowSO onboardingFlowScript;
    [SerializeField] private QuestDatabaseSO questDatabase;

    public override void InstallBindings()
    {
        #region PlayerPrefab
        Container.Bind<ITutorialProgressService>().To<TutorialProgressService>().AsSingle();
        Container.Bind<ProjectGameActionContext>().AsSingle();
        Container.Bind<PendingTutorialContext>().AsSingle();

        Container.Bind<ITutorialOverrideService>().To<TutorialOverrideService>().AsSingle();

        Container.BindInstance(questDatabase).IfNotBound();
        Container.BindInterfacesAndSelfTo<QuestManager>().AsSingle().NonLazy();
        #endregion


        #region GameFlow Services
        Container.Bind<GameSessionStateMachine>()
         .FromComponentInHierarchy()
         .AsSingle()
         .NonLazy();
        Container.Bind<GameSessionSceneRouter>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<SceneTransitionTracker>().AsSingle();

        Container.BindInterfacesAndSelfTo<GlobalFlowStateTracker>().AsSingle();

        Container.Bind<GameModeStateMachine>().AsSingle();

        Container.BindInterfacesAndSelfTo<FlowTextLocalization>().AsSingle();
        Container.BindInterfacesAndSelfTo<TurnManager>().AsSingle();

        Container.Bind<CoroutineService>().FromComponentInHierarchy().AsSingle();
        #endregion


        #region Domain Services
        Container.Bind<IEventHandlerRegistry>().To<EventHandlerRegistry>().AsSingle();
        Container.Bind<IDomainEventDispatcher>().To<DomainEventDispatcher>().AsSingle();
        Container.Bind<ICombatModifierService>().To<CombatModifierService>().AsSingle();

        Container.BindInterfacesAndSelfTo<MGFContext>().AsSingle();
        Container.Bind<BattleContextHolder>().AsSingle();
        Container.Bind<ShopContextHolder>().AsSingle();

        // Domain Event
        Container.BindInterfacesAndSelfTo<CardEventHandler>().AsSingle();
        Container.BindInterfacesAndSelfTo<ResourceEventHandler>().AsSingle().NonLazy();
        #endregion


        #region Story Chapter Services
        Container.Bind<StoryProgressModel>().AsSingle();
        Container.Bind<StoryProgressManager>().AsSingle(); 
        #endregion


        #region Deck Services
        // Domain and Repository
        Container.Bind<IHandDeckRepository>().To<HandDeckRepository>().AsSingle();

        Container.Bind<HandDeck>()
           .FromMethod(LoadOrCreateHandDeck)
           .AsSingle()
           .NonLazy();

        Container.Bind<CardConfig[]>().FromMethod(ctx =>
        {
            return Resources.LoadAll<CardConfig>("Card/Data");  // 載入整個文件夾
        })
        .AsSingle();

        Container.Bind<ICardConfigProvider>().To<CardConfigProvider>().AsSingle();
        Container.Bind<IPieceIconResolver>().To<PieceIconResolver>().AsSingle();


        // Application Services
        Container.Bind<IHandService>().To<HandService>().AsSingle();
        Container.BindInterfacesAndSelfTo<DeckService>().AsSingle();
        Container.BindInterfacesAndSelfTo<DeckInitializeService>().AsSingle();

        Container.BindInterfacesAndSelfTo<CardDataFactory>().AsSingle();
        Container.Bind<CardStaticDataRegistry>().AsSingle().NonLazy();
        #endregion


        #region PlayerInventory Service
        // Resource System
        Container.Bind<PlayerResource>().AsSingle();
        Container.BindInterfacesAndSelfTo<PlayerResourceService>().AsSingle();
        #endregion


        #region Environment Settings
        Container.Bind<GameSettingManager>()
            .FromInstance(FindFirstObjectByType<GameSettingManager>())
            .AsSingle()
            .WithArguments(audioMixer)
            .NonLazy();

        Container.Bind<RunProgress>().AsSingle();
        Container.BindInterfacesAndSelfTo<RunProgressService>().AsSingle().NonLazy();
        #endregion


        #region Guide Service
        Container.Bind<IGuideDataService>().To<GuideDataService>().AsSingle();
        Container.Bind<GuideRuntimeModel>().AsSingle();
        #endregion

        Container.BindInterfacesAndSelfTo<LootResolutionService>().AsSingle();

        #region Signals
        SignalBusInstaller.Install(Container);
        // Card Signals
        Container.DeclareSignal<CardsLoadedSignal>().OptionalSubscriber();
        Container.DeclareSignal<CardAddedSignal>().OptionalSubscriber();
        Container.DeclareSignal<CardRemovedSignal>().OptionalSubscriber();
        Container.DeclareSignal<HandCardsChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<CardStatusChangedSignal>().OptionalSubscriber();

        // Piece Signal
        Container.DeclareSignal<PieceMovedSignal>().OptionalSubscriber();
        Container.DeclareSignal<PieceSpawnedSignal>().OptionalSubscriber();
        Container.DeclareSignal<PieceRemovedSignal>().OptionalSubscriber();
        Container.DeclareSignal<PieceRestoredSignal>().OptionalSubscriber();
        Container.DeclareSignal<PieceTeamChangedSignal>().OptionalSubscriber();

        // Resource Signals
        Container.DeclareSignal<ResourceChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<EnemyDifficultyDeterminedSignal>().OptionalSubscriber();

        // GameFlow Signals
        Container.DeclareSignal<FlowChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<GlobalTurnChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<BoardInitializedSignal>().OptionalSubscriber();
        Container.DeclareSignal<ShopInitializedSignal>().OptionalSubscriber();
        Container.DeclareSignal<HandInitializedSignal>().OptionalSubscriber();
        Container.DeclareSignal<SceneUIReadySignal>().OptionalSubscriber();
        Container.DeclareSignal<GameEndedSignal>().OptionalSubscriber();
        Container.DeclareSignal<ShowGameEndUISignal>().OptionalSubscriber();

        // Story Chapter Signals
        Container.DeclareSignal<StoryChapterCompletedSignal>().OptionalSubscriber();

        // Tutorial Progress Signals
        Container.DeclareSignal<GameFlagAddedSignal>().OptionalSubscriber();
        Container.DeclareSignal<PlayTutorialSequenceSignal>().OptionalSubscriber();
        Container.DeclareSignal<LockRoutingSignal>().OptionalSubscriber();
        Container.DeclareSignal<UnlockRoutingSignal>().OptionalSubscriber();
        Container.DeclareSignal<QuestStateChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<FeatureLockChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<ForceBattleResetSignal>().OptionalSubscriber();
        Container.DeclareSignal<RewindSequenceFinishedSignal>().OptionalSubscriber();
        Container.DeclareSignal<BattleResetCompletedSignal>().OptionalSubscriber();
        Container.DeclareSignal<CameraModeRequestSignal>().OptionalSubscriber();
        Container.DeclareSignal<CameraFocusRequestSignal>().OptionalSubscriber();
        Container.DeclareSignal<CameraLockStateSignal>().OptionalSubscriber();
        Container.DeclareSignal<ResetCameraToDefaultSignal>().OptionalSubscriber();

        // Scene Transition Signals
        Container.DeclareSignal<BattleStartSignal>().OptionalSubscriber();
        Container.DeclareSignal<RequestEnterBattleSignal>().OptionalSubscriber();
        Container.DeclareSignal<BattleResolveSignal>().OptionalSubscriber();
        Container.DeclareSignal<RequestEnterShopSignal>().OptionalSubscriber();

        Container.DeclareSignal<StartGameCommandSignal>().OptionalSubscriber();
        Container.DeclareSignal<SceneReactivatedSignal>().OptionalSubscriber();
        Container.DeclareSignal<RequestSessionStateChangeSignal>().OptionalSubscriber();
        Container.DeclareSignal<GameSessionStateChangedSignal>().OptionalSubscriber();
        Container.DeclareSignal<ChangeBattleTypeSignal>().OptionalSubscriber();
        Container.DeclareSignal<SceneTransitionCompletedSignal>().OptionalSubscriber();

        Container.DeclareSignal<RandomEventConfirmedSignal>().OptionalSubscriber();
        Container.DeclareSignal<RandomEventTriggeredSignal>().OptionalSubscriber();
        Container.DeclareSignal<CombatModifierAddedSignal>().OptionalSubscriber();
        Container.DeclareSignal<CombatModifierTickedSignal>().OptionalSubscriber();

        #endregion
    }

    #region Factory Methods
    private HandDeck LoadOrCreateHandDeck(InjectContext context)
    {
        var repository = context.Container.Resolve<IHandDeckRepository>();
        return repository.Load();
    }
    #endregion
}

