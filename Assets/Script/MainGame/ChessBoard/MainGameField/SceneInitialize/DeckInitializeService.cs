using System;
using UnityEngine;
using Zenject;

public class DeckInitializeService : IDeckInitializeService, IInitializable, IDisposable
{
    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly IDeckService _deckService;
    [Inject] private readonly IHandService _handService;

    private bool _boardReady = false;
    private bool _uiReady = false;

    public void Initialize()
    {
        _signalBus.Subscribe<BoardInitializedSignal>(OnBoardInitialized);
        _signalBus.Subscribe<SceneUIReadySignal>(OnSceneUIReady);
        _signalBus.Subscribe<StartGameCommandSignal>(OnStartGameCommand);
    }

    private void OnBoardInitialized()
    {
        Debug.Log($"[DeckInitialize] 收到 BoardInitializedSignal - Time: {Time.time}");
        Debug.Log("[DeckInitialize] 棋盤已準備好");
        _boardReady = true;
        InitializeHand(Team.White);
    }

    private void OnSceneUIReady()
    {
        Debug.Log("[DeckInitialize] UI 已準備好");
        _uiReady = true;
        InitializeHand(Team.White);
    }

    private void OnStartGameCommand(StartGameCommandSignal signal)
    {
        if(signal.Mode != GameStartMode.LoadGame)
        {
            Debug.Log($"[DeckInitialize] 收到 StartGameCommandSignal");
            _deckService.Reset();
        }
        Debug.Log("[DeckInitialize] 收到 StartGameCommandSignal");
        InitializeHand(Team.White);
    }

    public void InitializeHand(Team team)
    {
        if (_boardReady && _uiReady)
        {
            // 1. 創建起始手牌資料
            _deckService.InitializeStartingHand(team);

            // 2. 驗證手牌狀態
            var handCards = _handService.GetHandCards();
            Debug.Log($"[HandInitialize] 手牌已創建，數量：{handCards.Count}/{_handService.GetMaxHandSize()}");

            // 3. 發送手牌初始化完成訊號
            _signalBus.Fire(new HandInitializedSignal(team));

            Debug.Log("[HandInitialize] 手牌系統初始化完成");
        }
    }

    public void Dispose()
    {
        _signalBus.TryUnsubscribe<BoardInitializedSignal>(OnBoardInitialized);
        _signalBus.TryUnsubscribe<SceneUIReadySignal>(OnSceneUIReady);
        _signalBus.TryUnsubscribe<StartGameCommandSignal>(OnStartGameCommand);
    }
}

public interface IDeckInitializeService
{
    void InitializeHand(Team team);
}

public class HandInitializedSignal
{
    public Team Team { get; }

    public HandInitializedSignal(Team team = Team.White)
    {
        Team = team;
    }
}