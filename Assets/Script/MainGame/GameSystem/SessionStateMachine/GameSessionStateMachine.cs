using System.Collections.Generic;
using UnityEngine;
using Zenject;

public enum GameSessionState
{
    MainMenu,
    Tutorial,
    Story,
    MainGame,
    Battle,
    Shop,
    GameOver,
    Ending
}

public interface IGameSessionState
{
    void OnEnter(GameSessionStateMachine machine);
    void OnExit(GameSessionStateMachine machine);
}

public class GameSessionStateMachine : MonoBehaviour
{
    private bool _isRoutingLocked = false;


    [Inject] private SignalBus _signalBus;
    [Inject] private ITutorialProgressService _tutorialService;
    [Inject] private StoryProgressManager _storyProgressManager;
    [Inject] private GameSessionSceneRouter _sceneRouter;
    [Inject] private BattleContextHolder _battleContextHolder;

    private Dictionary<GameSessionState, IGameSessionState> _states;
    private IGameSessionState _currentState;
    
    private GameSessionState _currentStateType;
    public GameSessionState CurrentState => _currentStateType;

    private Stack<GameSessionState> _stateHistory = new Stack<GameSessionState>();

    #region 初始化
    [Inject]
    public void Construct()
    {
        _states = new Dictionary<GameSessionState, IGameSessionState>
        {
            { GameSessionState.MainMenu, new MainMenuSessionState() },
            { GameSessionState.Story,    new StorySessionState()    },
            { GameSessionState.MainGame, new MainGameSessionState() },
            { GameSessionState.Battle,   new BattleSessionState()   },
            { GameSessionState.Shop,     new ShopSessionState()     },
            { GameSessionState.Ending, new EndingSessionState()   },
        };
    }

    private void Start()
    {
        _signalBus.Subscribe<RequestSessionStateChangeSignal>(OnStateChangeRequested);

        _signalBus.Subscribe<StartGameCommandSignal>(OnStartGameIntentReceived);
        _signalBus.Subscribe<RequestEnterBattleSignal>(OnBattleEnterRequested);
        _signalBus.Subscribe<ChangeBattleTypeSignal>(OnBattleTypeChangeRequested);
        _signalBus.Subscribe<BattleResolveSignal>(OnBattleEnded);
        _signalBus.Subscribe<RequestEnterShopSignal>(OnShopIntentReceived);
        _signalBus.Subscribe<StoryChapterCompletedSignal>(OnChapterCompleted);

        _signalBus.Subscribe<LockRoutingSignal>(() => _isRoutingLocked = true);
        _signalBus.Subscribe<UnlockRoutingSignal>(() => _isRoutingLocked = false);

        // TransitionToState(GameSessionState.MainMenu);
        SetInitialState(GameSessionState.MainMenu);
    }

    private void OnDestroy()
    {
        _signalBus?.Unsubscribe<RequestSessionStateChangeSignal>(OnStateChangeRequested);

        _signalBus?.Unsubscribe<StartGameCommandSignal>(OnStartGameIntentReceived);
        _signalBus?.Unsubscribe<RequestEnterBattleSignal>(OnBattleEnterRequested);
        _signalBus?.Unsubscribe<ChangeBattleTypeSignal>(OnBattleTypeChangeRequested);
        _signalBus?.Unsubscribe<BattleResolveSignal>(OnBattleEnded);
        _signalBus?.Unsubscribe<RequestEnterShopSignal>(OnShopIntentReceived);
        _signalBus?.Unsubscribe<StoryChapterCompletedSignal>(OnChapterCompleted);
    }

    private void SetInitialState(GameSessionState state)
    {
        _currentStateType = state;
        _currentState = _states[state];
        // 不呼叫 OnEnter，不切場景
    }
    #endregion

    #region 事件處理器
    private void OnStateChangeRequested(RequestSessionStateChangeSignal signal)
     => TransitionToState(signal.TargetState);


    private void OnStartGameIntentReceived(StartGameCommandSignal signal)
    {
        if (signal.Mode == GameStartMode.NewGame)
        {
            // 1. 初始化所有進度數據
            PlayerPrefs.DeleteAll();
            _storyProgressManager.ResetProgress();
            _tutorialService.ResetProgress();

            // 打上新遊戲的意圖，觸發序章任務 (Quest_01)
            _tutorialService.MarkStepCompleted("Intent_NewGame");
        }
        else if(signal.Mode == GameStartMode.LoadGame) 
        {
            // 1. 讀取上次離開時的狀態（預設為 MainGame）
            string savedStateStr = PlayerPrefs.GetString("Save_LastSessionState", GameSessionState.MainGame.ToString());

            if (System.Enum.TryParse(savedStateStr, out GameSessionState savedState))
            {
                Debug.Log($"[GSSM] 讀取遊戲，恢復狀態至: {savedState}");

                // 標記讀檔意圖 (可選，若有特殊任務需要在讀檔時觸發才用到)
                _tutorialService.MarkStepCompleted("Intent_LoadGame");

                // 2. 直接切換到該場景，不依賴任務系統的 StartAction
                TransitionToState(savedState);
            }
        }
        else if (signal.Mode == GameStartMode.DebugStart)
        {
            // 直接切到 MainGame，跳過序章
            PlayerPrefs.DeleteAll();
        }
    }


    private void OnBattleEnterRequested(RequestEnterBattleSignal signal)
    {
        if (signal.PendingBattle != null)
        {
            if (string.IsNullOrEmpty(signal.PendingBattle.BattleId))
            {
                signal.PendingBattle.BattleId = System.Guid.NewGuid().ToString();
            }

            _battleContextHolder.SetBattle(signal.PendingBattle);
        }

        if (_states[GameSessionState.Battle] is BattleSessionState b)
        {
            b.SetBattleType(signal.BattleType);
        }

        TransitionToState(GameSessionState.Battle);
    }

    private void OnBattleTypeChangeRequested(ChangeBattleTypeSignal signal)
    {
        if (_currentStateType != GameSessionState.Battle) return;

        if (_states[GameSessionState.Battle] is BattleSessionState b)
            b.SetBattleType(signal.NewBattleType);

        /// 直接繞過 TransitionToState , 未來需注意debug時的內容 ///
        _sceneRouter.Route(GameSessionState.Battle, GameSessionState.Battle, signal.NewBattleType);
    }

    private void OnBattleEnded(BattleResolveSignal signal)
    {
        BackToState();
    }


    private void OnShopIntentReceived(RequestEnterShopSignal signal)
    {
        //if (IsOnboardingHandlingRouting) return;
        if (_isRoutingLocked)
        {
            Debug.Log("[GSSM] 任務鎖定中，禁止隨意切換狀態");
            return;
        }

        TransitionToState(GameSessionState.Shop);
    }


    private void OnChapterCompleted(StoryChapterCompletedSignal signal)
    {
        var completedChapter = _storyProgressManager.GetCurrentChapter();
    }
    #endregion

    #region 核心方法：狀態轉換
    public void TransitionToState(GameSessionState next, bool pushHistory = true)
    {
        if (!_states.ContainsKey(next)) return;
        var prev = _currentStateType;

        // 儲存離開遊戲時的狀態（如果不是回到主選單）
        //if (next != GameSessionState.MainMenu)
        //{
        //    PlayerPrefs.SetString("Save_LastSessionState", next.ToString());
        //    PlayerPrefs.Save();
        //}

        // ── 步驟 1：告知舊狀態退場 ──
        _currentState?.OnExit(this);

        // ── 步驟 2：把舊狀態存入歷史（為了 GoBack） ──
        if (pushHistory && _currentState != null)
            _stateHistory.Push(prev);

        // ── 步驟 3：從電話簿找到新狀態物件 ──
        var battleType = (_states.TryGetValue(GameSessionState.Battle, out var bs) && bs is BattleSessionState b)
           ? b.CurrentBattleType
           : BattleType.Normal;
        _sceneRouter.Route(prev, next, battleType);

        _currentStateType = next;
        _currentState = _states[next];

        // ── 步驟 4：告知新狀態上台 ──
        _currentState.OnEnter(this);

        // ── 步驟 5：廣播「狀態已改變」訊號 ──
        // HUD、存檔、音效等系統會收到這個廣播並自行處理
        _signalBus.Fire(new GameSessionStateChangedSignal
        {
            PreviousState = prev,
            NextState = next
        });
    }

    public void BackToState()
    {
        if (_stateHistory.Count == 0) return; // 沒有歷史，不動作
        TransitionToState(_stateHistory.Pop(), pushHistory: false);
    }
    #endregion

    public BattleType GetCurrentBattleType()
    {
        // 先從字典中安全地取出 BattleSessionState
        if (_states.TryGetValue(GameSessionState.Battle, out var state) && state is BattleSessionState battleState)
        {
            return battleState.CurrentBattleType;
        }

        return BattleType.Normal; // 預設值防呆
    }
}
