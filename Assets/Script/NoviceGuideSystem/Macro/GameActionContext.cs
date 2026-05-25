using System.Collections.Generic;
using UnityEngine;
using Zenject;

#region Global
public class ProjectGameActionContext
{
    public SignalBus SignalBus { get; }

    // 遊戲進度相關 (GameFlag)
    public ITutorialOverrideService TutorialOverrideService { get; }
    public ITutorialProgressService TutorialProgressService { get; }
    public PendingTutorialContext PendingTutorialContext { get; }

    // 故事進度相關 
    public StoryProgressManager StoryProgressManager { get; }

    // 場景管理相關
    public GameSessionStateMachine GameSessionStateMachine { get; }
    public SceneTransitionTracker SceneTransitionTracker { get; }
    public GlobalFlowStateTracker GlobalFlowStateTracker { get; }
    public MGFContext MGFContext { get; }
    public BattleContextHolder BattleContext { get; }

    // 手牌與卡牌庫相關
    public IHandService HandService { get; }

    [Inject]
    public ProjectGameActionContext(
       SignalBus signalBus,
       ITutorialOverrideService overrideService,
       ITutorialProgressService progressService,
       PendingTutorialContext pendingContext,
       StoryProgressManager storyManager,
       GameSessionStateMachine stateMachine,
       SceneTransitionTracker sceneTransitionTracker,
       GlobalFlowStateTracker globalFlowStateTracker,
       MGFContext mgfContext,
       BattleContextHolder battleContextHolder,
       IHandService handService
       )
    {
        // 在建構瞬間完成所有依賴的緩存
        SignalBus = signalBus;
        TutorialOverrideService = overrideService;
        TutorialProgressService = progressService;
        PendingTutorialContext = pendingContext;
        StoryProgressManager = storyManager;
        GameSessionStateMachine = stateMachine;
        SceneTransitionTracker = sceneTransitionTracker;
        GlobalFlowStateTracker = globalFlowStateTracker;
        MGFContext = mgfContext;
        BattleContext = battleContextHolder;
        HandService = handService;
    }
}
#endregion

#region Local
public class SceneGameActionContext
{
    // 取得全域 Context 的參考，方便微觀 Action 存取全域狀態
    public ProjectGameActionContext Global { get; }
    public SignalBus SignalBus { get; }

    // 場景專屬 UI 相關 (可能會是 null)
    public IDialogueBoxManager DialogueBoxManager { get; }
    public TutorialHighlightService TutorialHighlightService { get; }
    public IArrowVisualizerService ArrowVisualizerService { get; }
    public TutorialTargetRegistry TargetRegistry { get; }

    // 場景專屬戰鬥與棋盤領域相關 (可能會是 null)
    public IBoardQueryService BoardQuery { get; }
    public IPieceViewRegistry PieceViewRegistry { get; }
    public ILandmarkViewRegistry LandmarkViewRegistry { get; }
    public ITilePointerService TilePointerService { get; }
    public IPieceSpawner PieceSpawner { get; }
    public IMoveHandler MoveHandler { get; }
    public IBoardVisualizer BoardVisualizer { get; }
    public ICardPlacementRule CardPlacementRule { get; }
    public IEnumerable<IPieceSelectionExtension> SelectionExtensions { get; }
    public IStandbyZone StandbyZone { get; }
    public ITargetPieceTracker TargetPieceTracker { get; }


    [Inject]
    public SceneGameActionContext(
        ProjectGameActionContext globalContext,
        SignalBus signalBus,

        [InjectOptional] IDialogueBoxManager dialogueBoxManager,
        [InjectOptional] TutorialHighlightService highlightService,
        [InjectOptional] IArrowVisualizerService arrowVisualizer,
        [InjectOptional] TutorialTargetRegistry targetRegistry,

        [InjectOptional] IBoardQueryService boardQuery,
        [InjectOptional] IPieceViewRegistry pieceViewRegistry,
        [InjectOptional] ILandmarkViewRegistry landmarkViewRegistry,
        [InjectOptional] ITilePointerService tilePointer,
        [InjectOptional] IPieceSpawner pieceSpawner,
        [InjectOptional] IMoveHandler moveHandler,
        [InjectOptional] IBoardVisualizer boardVisualizer,
        [InjectOptional] ICardPlacementRule cardPlacementRule,
        [InjectOptional] IEnumerable<IPieceSelectionExtension> selectionExtensions,
        [InjectOptional] IStandbyZone standbyZone,
        [InjectOptional] ITargetPieceTracker targetPieceTracker
        )
    {
        Global = globalContext;
        SignalBus = signalBus;

        DialogueBoxManager = dialogueBoxManager;
        TutorialHighlightService = highlightService;
        ArrowVisualizerService = arrowVisualizer;
        TargetRegistry = targetRegistry;

        BoardQuery = boardQuery;
        PieceViewRegistry = pieceViewRegistry;
        LandmarkViewRegistry = landmarkViewRegistry;
        TilePointerService = tilePointer;
        PieceSpawner = pieceSpawner;
        MoveHandler = moveHandler;
        BoardVisualizer = boardVisualizer;
        CardPlacementRule = cardPlacementRule;
        SelectionExtensions = selectionExtensions;
        StandbyZone = standbyZone;
        TargetPieceTracker = targetPieceTracker;
    }
}
#endregion
