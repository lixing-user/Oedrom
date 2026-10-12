using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class MainSecPhase : IGamePhaseState
{
    [Inject] private readonly DiContainer _container;
    [Inject] private readonly CoroutineService _coroutineService;

    private GamePhaseStateMachine _machine;
    private GamePhaseModel _model;
    private GamePhaseFacade _facade;
    private SignalBus _signalBus;
    private BattleContextHolder _battleContext;
    private IMainSecPhaseStrategy _currentStrategy;

    [Inject] private List<IMainSecPhaseStrategy> _strategies;
    [Inject] private ICameraPresenter _cameraPresenter;
    [Inject] private IPieceViewRegistry _viewRegistry;

    private bool _isWaitingForEventUI = false;
    private bool _isWaitingForSceneReturn = false;
    private ResolutionType _triggeredEventType;
    private GameSessionState _lastPreviousState;

    #region 介面實作
    public void OnEnter(GamePhaseStateMachine machine)
    {
        _machine = machine;
        _model = machine.GetModel();
        _facade = machine.GetFacade();
        _signalBus = machine.GetSignalBus();
        _battleContext = _container.Resolve<BattleContextHolder>();

        _currentStrategy = _strategies.FirstOrDefault(s => s.TargetTeam == _facade.GetCurrentTeam());

        Debug.Log("[MainPhase2] === 進入主要階段2 ===");

        _signalBus.Subscribe<SceneReactivatedSignal>(OnSceneReactivated);
        _signalBus.Subscribe<GameSessionStateChangedSignal>(OnSessionStateChanged);
        _signalBus.Subscribe<RandomEventConfirmedSignal>(OnEventConfirmed);

        _model.CanProgressToNextPhase = false;

        _coroutineService.StartCoroutine(ExecutePhaseCoroutine());
    }

    public void OnUpdate(GamePhaseStateMachine machine){}

    public void OnExit(GamePhaseStateMachine machine)
    {
        Debug.Log("[MainPhase2] === 離開主要階段2 ===");

        _signalBus.Unsubscribe<SceneReactivatedSignal>(OnSceneReactivated);
        _signalBus.Unsubscribe<GameSessionStateChangedSignal>(OnSessionStateChanged);
        _signalBus.Unsubscribe<RandomEventConfirmedSignal>(OnEventConfirmed);

        _cameraPresenter.LockCamera("MainSecPhase_Focus", false);
        _facade.DisableAllInput();
    }

    private IEnumerator ExecutePhaseCoroutine()
    {
        while (!_machine.gameObject.activeSelf)
        {
            yield return null;
        }

        int myVersion = _machine.PhaseVersion;
        Debug.Log("[MainPhase2] 開始執行階段流程...");
        _facade.DisableAllInput();

        // === 步驟 1：檢測事件類型 ===
        ResolutionType eventType = DetermineEventType();
        bool isAI = _facade.GetCurrentTeam() == Team.Black;

        if (eventType == ResolutionType.RandomEvent && !isAI)
        {
            _model.EventTriggerPieceId = _model.LastMovedPieceId;
            _model.PendingRandomEvent = _facade.RollRandomEvent(_model.EventTriggerPieceId);

            if (_model.PendingRandomEvent == null)
                eventType = ResolutionType.None;
        }

        _triggeredEventType = eventType;
        _model.CurrentResolutionType = eventType;

        // === 步驟 2：根據事件類型觸發對應流程 ===
        if (eventType == ResolutionType.None)
        {
            Debug.Log("[MainPhase2] 無事件觸發，直接進入結束階段");
            SwitchToNextPhase();
            yield break; // 直接結束協程
        }

        // AI 無 UI 事件攔截：踩到商店/隨機事件，直接切換，攝影機不動
        if (isAI && (eventType == ResolutionType.Shop || eventType == ResolutionType.RandomEvent))
        {
            Debug.Log($"[MainPhase2] AI 觸發 {eventType}，無 UI 演出，直接進入下一階段");
            SwitchToNextPhase();
            yield break;
        }

        // === 步驟 3：【視覺演出】確定有事件，才拉近鏡頭並鎖定 ===
        FocusCameraOnMovedPiece();
        yield return new WaitForSeconds(0.6f);

        // === 步驟 4：執行事件流程 ===
        if (eventType == ResolutionType.Battle || eventType == ResolutionType.Shop)
        {
            if (_facade.GetCurrentTeam() == Team.Black && eventType == ResolutionType.Shop)
            {
                Debug.Log("[MainPhase2] AI 踩到商店，不執行場景切換，直接進入結算");
                SwitchToNextPhase();
                yield break;
            }
            // 玩家或是戰鬥事件，執行策略
            yield return _currentStrategy.Trigger(eventType, _model);
            if (!_machine.IsCurrent(myVersion)) yield break;
            yield return WaitForSceneReturn(myVersion);
            if (!_machine.IsCurrent(myVersion)) yield break;
        }
        else if (eventType == ResolutionType.RandomEvent)
        {
            // 隨機事件：只等待 UI 關閉，不等待場景
            if (_facade.GetCurrentTeam() == Team.White)
            {
                yield return ExecuteRandomEventFlow(myVersion);
                if (!_machine.IsCurrent(myVersion)) yield break;
            }
                
            if (_triggeredEventType != ResolutionType.Battle)   // Battle事件的 SwitchToNextPhase 會交由 OnSceneReactivated 去呼叫
            {
                SwitchToNextPhase();
            }
        }
    }

    private void SwitchToNextPhase()
    {
        // === 步驟 4：標記完成 ===
        _model.CanProgressToNextPhase = true;

        // === 步驟 5：進入下一階段 ===
        _machine.ChangePhase(GamePhaseState.ResolutionPhase);

        Debug.Log("[MainPhase2] === 主要階段2結束，進入處理階段 ===");
    }

    private IEnumerator WaitForSceneReturn(int myVersion)
    {
        Debug.Log("[MainPhase2] 等待場景返回...");
        _isWaitingForSceneReturn = true;
        while (_isWaitingForSceneReturn && _machine.IsCurrent(myVersion))
            yield return null;
        _isWaitingForSceneReturn = false; // 確保被強制中止時旗標也復位
    }
    #endregion


    private IEnumerator ExecuteRandomEventFlow(int myVersion)
    {
        Debug.Log("[MainPhase2] 執行隨機事件流程...");

        if (_model.PendingRandomEvent == null)
        {
            Debug.Log("[MainPhase2] RandomEventManager 未抽中任何事件，跳過 UI 等待環節");
            _isWaitingForEventUI = false; // 確保狀態正確
            yield break; // 直接結束協程，讓主流程繼續執行 SwitchToNextPhase()
        }

        // 2. 掛起協程，等待玩家點擊 UI 確認
        _isWaitingForEventUI = true;
        _facade.ShowRandomEventUI(_model.PendingRandomEvent);
        while (_isWaitingForEventUI)
        {
            yield return null;
        }

        #region 戰鬥事件
        var selectedOption = _model.PendingRandomEvent.options[_model.SelectedEventOptionIndex];

        bool isBattleEvent = selectedOption.effects.Any(e => e.effectType == EventEffectType.TriggerBattle);

        if (isBattleEvent)
        {
            Debug.Log("[MainPhase2] 隨機事件引發了戰鬥！切換為戰鬥流程...");

            // 提早執行事件 (這會觸發 TriggerBattleEffect，並發射切換場景的信號)
            _facade.ResolveRandomEvent(_model.PendingRandomEvent, _model.SelectedEventOptionIndex, _model.EventTriggerPieceId);

            // 清理暫存，避免 ResolutionPhase 等一下又重複執行一次
            _model.PendingRandomEvent = null;
            _model.EventTriggerPieceId = null;
            _model.SelectedEventOptionIndex = -1;

            // 改變狀態機的認知，偽裝成普通戰鬥！
            _triggeredEventType = ResolutionType.Battle;
            _model.CurrentResolutionType = ResolutionType.Battle;

            // 轉交給現有的場景等待機制
            yield return WaitForSceneReturn(myVersion);
            if (!_machine.IsCurrent(myVersion)) yield break;

            // 結束協程。等場景載回來時，OnSceneReactivated 會接手呼叫 SwitchToNextPhase()
            yield break;
        }
        #endregion

        // 3. UI 關閉，進入結算階段
        _model.CurrentResolutionType = ResolutionType.RandomEvent;
        _model.CanProgressToNextPhase = true;

        yield return new WaitForSeconds(1.0f);
        Debug.Log("[MainPhase2] 隨機事件結束，進入下一階段");
    }


    #region Step1：識別事件種類並觸發相應內容
    private ResolutionType DetermineEventType()
    {
        if (!_model.HasMovedThisTurn || _model.LastMovedPiecePosition == null)
        {
            Debug.Log("[MainPhase2] 本回合未移動，無事件");
            return ResolutionType.None;
        }

        // 優先級 1：戰鬥
        if (_model.IsBattleTriggered)
        {
            Debug.Log("[MainPhase2] 檢測到戰鬥標記");
            return ResolutionType.Battle;
        }

        // 優先級 2：檢查是否在移動階段踩到了地標/商店
        if (_model.CurrentResolutionType == ResolutionType.Shop)
        {
            Debug.Log("[MainPhase2] 檢測到商店事件標記");
            return ResolutionType.Shop;
        }

        // 優先級 3：檢查是否觸發隨機事件
        if (_model.IsRandomEventTriggered)
        {
            Debug.Log("[MainPhase2] 檢測到隨機事件標記");
            return ResolutionType.RandomEvent;
        }

        // 優先級 4：檢查普通移動事件
        if (!_model.HasMovedThisTurn || _model.LastMovedPiecePosition == null)
        {
            Debug.Log("[MainPhase2] 本回合未移動，無事件");
            return ResolutionType.None;
        }

        Debug.Log("[MainPhase2] 移動,無特殊事件");
        return ResolutionType.None;
    }
    #endregion

    #region Step2：接收信號後搜集資訊，並傳遞給下一個Phase處理實作
    // === 1. 邏輯守門員：只負責記錄我們從哪裡回來 ===
    private void OnSessionStateChanged(GameSessionStateChangedSignal signal)
    {
        if (signal.NextState == GameSessionState.MainGame)
        {
            _lastPreviousState = signal.PreviousState;
        }
    }

    // === 2. 視覺發令槍：畫面亮起時才執行，並檢查邏輯是否合法 ===
    private void OnSceneReactivated(SceneReactivatedSignal signal)
    {
        if (signal.FromScene != SceneDatabase.SceneName.MainGameField)
        {
            Debug.Log($"[MainPhase2] 忽略 SceneReactivatedSignal，因為現在是進入 {signal.FromScene}，我們還在等待玩家回來。");
            return;
        }

        // 2. 核心防護：這是我主動發起的場景切換（戰鬥/商店）嗎？
        if (!_isWaitingForSceneReturn)
        {
            Debug.Log($"[MainPhase2] 場景喚醒，但 MainSecPhase 並未在等待場景返回。這可能是 Quest 觸發的劇情切換結束，忽略此訊號，保持在 MainSecPhase。");
            return; // return 後，原本掛起的 ExecutePhaseCoroutine 就會繼續停在它該在的地方（例如等待隨機事件 UI 關閉）
        }

        // 【核心防禦】檢查守門員的紀錄：我們是不是被 Quest 劇情劫持了？
        if (_triggeredEventType == ResolutionType.Battle && _lastPreviousState != GameSessionState.Battle)
        {
            Debug.Log($"[MainPhase2] 場景喚醒！但我們是從 {_lastPreviousState} 回來的(可能是任務劇情)，忽略此視覺信號，繼續等待 Battle。");
            return;
        }

        if (_triggeredEventType == ResolutionType.Shop && _lastPreviousState != GameSessionState.Shop)
        {
            Debug.Log($"[MainPhase2] 場景喚醒！但我們是從 {_lastPreviousState} 回來的，忽略此視覺信號，繼續等待 Shop。");
            return;
        }

        Debug.Log($"[MainPhase2] 場景從 {signal.FromScene} 重新激活:");
        _isWaitingForSceneReturn = false;

        switch (_triggeredEventType)
        {
            case ResolutionType.Battle:
                _model.PendingBattle = _battleContext.CurrentBattle;

                _model.PendingBattleResult = BattleResultData.FromBattleContext(
                _battleContext.CurrentBattle,
                _battleContext.BattleWinner.Value);

                Debug.Log($"[MainPhase2] 已讀取戰鬥結果: Winner={_model.PendingBattleResult?.WinnerTeam}");
                break;

            case ResolutionType.Shop:
                Debug.Log($"[MainPhase2] 已讀取商店結果");
                break;
        }

        SwitchToNextPhase();
    }

    private void OnEventConfirmed(RandomEventConfirmedSignal signal)
    {
        _model.SelectedEventOptionIndex = signal.SelectedOptionIndex;

        Debug.Log($"[MainPhase2] 玩家確認了事件，選項為: {signal.SelectedOptionIndex}");
        _isWaitingForEventUI = false; // 解除等待
    }
    #endregion

    private void FocusCameraOnMovedPiece() 
    {
        if (_model.LastMovedPieceId != null)
        {
            var pieceView = _viewRegistry.GetView(_model.LastMovedPieceId);
            if (pieceView != null)
            {
                _cameraPresenter.RequestPieceFocusMode(pieceView.transform);
                Debug.Log($"[MainPhase2] 攝影機切換至 Focus 模式，聚焦棋子: {_model.LastMovedPieceId}");

                _cameraPresenter.LockCamera("MainSecPhase_Focus", true);
            }
        }
    }
}

