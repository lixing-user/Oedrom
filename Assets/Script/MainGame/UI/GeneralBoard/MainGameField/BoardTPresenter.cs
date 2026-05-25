using System;
using UnityEngine;
using Zenject;

public class BoardTPresenter : MonoBehaviour
{
    [Inject] private GamePhaseModel _model;
    [Inject] private SignalBus _signalBus;
    private IBoardTView view;


    #region 初始化&綁定
    void Start()
    {
        view = GetComponent<IBoardTView>();

        _signalBus.Subscribe<PhaseChangedSignal>(OnPhaseChangedSignal);
        _signalBus.Subscribe<RandomEventTriggeredSignal>(OnRandomEventDataReceived);
        _signalBus.Subscribe<ResolutionTriggeredSignal>(OnResolutionTriggered);

        BindViewEvents();
    }
    void OnDestroy()
    {
        _signalBus?.Unsubscribe<PhaseChangedSignal>(OnPhaseChangedSignal);
        _signalBus?.Unsubscribe<RandomEventTriggeredSignal>(OnRandomEventDataReceived);
        _signalBus?.Unsubscribe<ResolutionTriggeredSignal>(OnResolutionTriggered);

        UnbindViewEvents();
    }


    private void BindViewEvents()
    {
        view.OnBackToLobbyRequest += OnBackToLobbyRequested;
        view.OnBackToLobbyConfirmed += OnBackToLobbyConfirmed;
        view.OnBackClicked += OnBackClicked;

        view.OnBattleConfirmed += OnBattleConfirmed;
        view.OnShopEntered += OnShopConfirmed;
        view.OnRandomEventOptionSelected += OnEventOptionSelected;
    }
    private void UnbindViewEvents()
    {
        view.OnBattleConfirmed -= OnBattleConfirmed;
        view.OnShopEntered -= OnShopConfirmed;
        view.OnRandomEventOptionSelected -= OnEventOptionSelected;
        view.OnBackToLobbyRequest -= OnBackToLobbyRequested;
        view.OnBackToLobbyConfirmed -= OnBackToLobbyConfirmed;
        view.OnBackClicked -= OnBackClicked;
    }


    private void OnResolutionTriggered(ResolutionTriggeredSignal signal)
    {
        Debug.Log($"[BoardTPresenter] 觸發結算 UI: {signal.Type}");
        HandleResolutionTypeChanged(signal.Type);  // 顯示對應面板
    }
    private void OnPhaseChangedSignal(PhaseChangedSignal signal)
    {
        Debug.Log($"[BoardTPresenter] 階段切換: {signal.PreviousPhase} → {signal.CurrentPhase}");
        HandlePhaseChanged(signal.CurrentPhase);
    }
    #endregion

    #region GamePhase狀態機回應
        #region 待擴充內容
    private void HandlePhaseChanged(GamePhaseState newState)
    {
        view.HideAllPanels();

        switch (newState)
        {
            case GamePhaseState.StandbyPhase:
                // 待機階段不顯示任何面板
                break;
            case GamePhaseState.MainPhase1:
                // 主要階段 1 可能顯示手牌提示或操作指引
                break;
            case GamePhaseState.MovementPhase:
                // 移動階段可能顯示移動範圍高亮
                break;
            case GamePhaseState.MainPhase2:
                // 主要階段 2 可能觸發事件
                break;
            case GamePhaseState.ResolutionPhase:
                // 處理階段，跳出panel
                break;
            case GamePhaseState.EndPhase:
                // 回合結束階段
                break;
        }
    }
    #endregion
    private void HandleResolutionTypeChanged(ResolutionType type)
    {
        view.HideAllPanels();

        switch (type)
        {
            case ResolutionType.Battle:
                view.ShowBattleEventPanel();
                break;

            case ResolutionType.Shop:
                view.ShowShopEventPanel();
                break;

            case ResolutionType.RandomEvent:
                Debug.Log("[BoardTPresenter] 進入隨機事件處理，等待資料中...");
                break;

            case ResolutionType.None:
                //其他效果
                break;
        }
    }
    #endregion

    #region 效果實作
    private void OnBattleConfirmed()
    {
        _signalBus.Fire(new RequestEnterBattleSignal {
            BattleType = _model.PendingBattle?.BattleType ?? BattleType.Normal,
            PendingBattle = _model.PendingBattle
        });
        Debug.Log("[BoardTPresenter] 請求進入戰鬥狀態");
        view.HideAllPanels();
    }

    private void OnShopConfirmed()
    {
        Debug.Log("[BoardTPresenter] 請求進入商店狀態");
        _signalBus.Fire<RequestEnterShopSignal>();
        view.HideAllPanels();
    }

    private void OnEventOptionSelected(int selectedIndex)
    {
        Debug.Log($"[BoardTPresenter] 玩家選擇了事件選項 Index: {selectedIndex}");
        _signalBus.Fire(new RandomEventConfirmedSignal(selectedIndex)); // 傳入 Index！
        view.HideAllPanels();
    }
    private void OnRandomEventDataReceived(RandomEventTriggeredSignal signal)
    {
        Debug.Log($"[BoardTPresenter] 收到事件資料: {signal.EventName}");

        // 1. 將資料填入 View
        view.SetRandomEventContent(signal.EventName, signal.Description, signal.Icon, signal.Options);

        // 2. 確保面板是開啟的 (如果 ResolutionTriggeredSignal 沒開到的話)
        view.ShowRandomEventPanel();
    }

    private void OnBackToLobbyRequested()
    {
        view.ShowBackToLobbyPanel();
    }

    private void OnBackToLobbyConfirmed()
    {
        _signalBus.Fire(new RequestSessionStateChangeSignal { 
            TargetState = GameSessionState.MainMenu 
        });
        Debug.Log("[BoardTPresenter] 請求返回主菜單");
        view.HideAllPanels();
    }

    private void OnBackClicked()
    {
        view.HideAllPanels();
    }
    #endregion
}
