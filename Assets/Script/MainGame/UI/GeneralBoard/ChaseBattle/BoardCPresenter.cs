using UnityEngine;
using Zenject;

public class BoardCPresenter : MonoBehaviour
{
    [Inject] private ITutorialOverrideService _overrideService;
    [Inject] private SignalBus _signalBus;
    [Inject] private BattleContextHolder _battleContextHolder;
    [Inject] private readonly IMoveCounterService _moveCounter;

    private BoardCView _view;

   
    void Start()
    {
        _view = GetComponent<BoardCView>();

        _signalBus.Subscribe<MoveCountChangedSignal>(OnMoveCountChanged);
        _signalBus.Subscribe<ShowGameEndUISignal>(OnBattleEnded);

        BindViewEvents();
        InitializeUIFromService();
    }

    private void OnDestroy()
    {
        _signalBus.TryUnsubscribe<MoveCountChangedSignal>(OnMoveCountChanged);
        _signalBus.TryUnsubscribe<ShowGameEndUISignal>(OnBattleEnded);
        UnbindViewEvents();
    }

    private void BindViewEvents()
    {
        _view.OnBackMainGameRequest += OnExitBattleRequest;
    }
    private void UnbindViewEvents()
    {
        _view.OnBackMainGameRequest -= OnExitBattleRequest;
    }


    private void OnBattleEnded(ShowGameEndUISignal signal)
    {
        var sessionData = _battleContextHolder.CurrentBattle;
        if (sessionData == null) return;

        _view.ShowMGFScenePanel();
    }


    private void OnExitBattleRequest()
    {
        Debug.Log("[BoardC] 收到退出戰鬥請求，返回主遊戲...");
        var sessionData = _battleContextHolder.CurrentBattle;
        if (sessionData == null)
        {
            Debug.LogError("[BoardNPresenter] 缺少戰鬥上下文！");
            return;
        }

        int winner = _battleContextHolder.BattleWinner.HasValue
            ? (int)_battleContextHolder.BattleWinner.Value
            : -1;

        _signalBus.Fire(new BattleResolveSignal(
            attackerId: sessionData.AttackerId,
            defenderId: sessionData.DefenderId)
        {
            AttackerType = sessionData.AttackerType,
            DefenderType = sessionData.DefenderType,
            AttackerTeam = sessionData.AttackerTeam,
            DefenderTeam = sessionData.DefenderTeam,
            AttackerPos = sessionData.AttackerPos,
            DefenderPos = sessionData.DefenderPos,
            WinnerTeam = winner,
            BattleType = sessionData.BattleType
        });

        //_signalBus.Fire(new SceneReactivatedSignal("NormalBattle"));
    }


    private void InitializeUIFromService()
    {
        if (_moveCounter == null) return;

        // 告訴 View 生成最大數量的 Token
        _view.InitializeTokens(_moveCounter.MaxMoves);

        // 告訴 View 根據當前狀態熄滅/點亮 Token
        _view.UpdateMovesDisplay(_moveCounter.CurrentMoves, _moveCounter.MaxMoves);
    }

    private void OnMoveCountChanged(MoveCountChangedSignal signal)
    {
        if (signal.Current == 0)
        {
            _view.InitializeTokens(signal.Max); // 遊戲剛開始，亮起所有 Token
        }

        _view.UpdateMovesDisplay(signal.Current, signal.Max);
    }
}
