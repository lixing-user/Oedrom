using System;
using UnityEngine;
using Zenject;

public interface IHandUIStateManager
{
    void RefreshAllCards();
    void UpdateHandInfo();
    void ToggleHandMode();
    void SetHandDisplayMode(HandDisplayMode mode);
}

public class HandUIStateManager : IHandUIStateManager, IInitializable, IDisposable, ITickable
{
    [Inject] private SignalBus _signalBus;
    [Inject] private IHandService _handManager;
    [Inject] private IHandCardContainer _handCardContainer;

    [Inject] private IBoardInputService _boardInputService;
    [Inject] private ICardInputManager _cardInputManager;


    private bool _isInitialized = false;

    private HandDisplayMode _currentMode = HandDisplayMode.Centered;
    private HandDisplayMode _preDragMode = HandDisplayMode.Centered;
    private UIVisibilityHeadupMode _currentVisibility = UIVisibilityHeadupMode.Standard;

    private bool _isHandUILocked = false;
    private bool _isSignalForcedSubtle = false; // 由教學或腳本控制
    private bool _isCameraForcedSubtle = false; // 由攝影機視角控制
    private bool _isDragForcedSubtle = false;   // 由玩家拖曳控制


    #region Lifecycle
    public void Initialize()
    {
        _signalBus.Subscribe<FeatureLockChangedSignal>(OnFeatureLockChanged);

        _signalBus.Subscribe<CameraModeChangedSignal>(OnCameraModeChanged);
        _signalBus.Subscribe<ToggleHandDeckSignal>(OnGameStarted);
        _signalBus.Subscribe<ChangeHandUIVisibilitySignal>(OnChangeHandUIVisibility);

        _signalBus.Subscribe<CardsLoadedSignal>(OnCardsLoaded);
        _signalBus.Subscribe<CardAddedSignal>(OnCardAdded);
        _signalBus.Subscribe<CardRemovedSignal>(OnCardRemoved);
        _signalBus.Subscribe<HandCardsChangedSignal>(OnHandCardsChanged);

        // 2. 訂閱 Container 的視覺就緒事件
        _handCardContainer.OnVisualLayoutReady += HandleVisualLayoutReady;

        // 3. 設定初始狀態
        RefreshAllCards();
        _currentMode = HandDisplayMode.Centered;
        _currentVisibility = UIVisibilityHeadupMode.Subtle;

        _handCardContainer.SetHandMode(_currentMode);
        _handCardContainer.SetVisibilityMode(_currentVisibility, immediate: true);

        _isInitialized = true;
    }

    public void Dispose()
    {
        _signalBus.Unsubscribe<FeatureLockChangedSignal>(OnFeatureLockChanged);
        _signalBus.Unsubscribe<CameraModeChangedSignal>(OnCameraModeChanged);
        _signalBus.Unsubscribe<ToggleHandDeckSignal>(OnGameStarted);
        _signalBus.Unsubscribe<ChangeHandUIVisibilitySignal>(OnChangeHandUIVisibility);
        _signalBus.Unsubscribe<CardsLoadedSignal>(OnCardsLoaded);
        _signalBus.Unsubscribe<CardAddedSignal>(OnCardAdded);
        _signalBus.Unsubscribe<CardRemovedSignal>(OnCardRemoved);
        _signalBus.Unsubscribe<HandCardsChangedSignal>(OnHandCardsChanged);

        _handCardContainer.OnVisualLayoutReady -= HandleVisualLayoutReady;
    }

    public void Tick()
    {
        if (!_isInitialized) return;
        HandleDragSubtleMode();
    }
    #endregion


    #region Signal Handlers
    private void OnGameStarted(ToggleHandDeckSignal signal)
    {
        _currentMode = HandDisplayMode.Centered;
        EvaluateUIState();
    }

    private void OnCameraModeChanged(CameraModeChangedSignal signal)
    {
        switch (signal.NewMode)
        {
            case CameraMode.Opening:
            case CameraMode.Freelook:
            case CameraMode.Focus:
            case CameraMode.FocusEnemy:
                // 進入這些模式時，強制切換為 Subtle 模式
                _isCameraForcedSubtle = true;
                break;

            case CameraMode.Default:
            case CameraMode.TopView:
            case CameraMode.Uninitialized:
            default:
                // 回到一般模式時，解除強制並恢復為 Standard 模式
                _isCameraForcedSubtle = false;
                break;
        }
        EvaluateUIState();
    }

    private void OnChangeHandUIVisibility(ChangeHandUIVisibilitySignal signal)
    {
        _isSignalForcedSubtle = (signal.VisibilityMode == UIVisibilityHeadupMode.Subtle);

        // 若要解除強制，可以同時套用腳本傳來的排版設定
        if (!_isSignalForcedSubtle)
        {
            _currentMode = signal.DisplayMode;
        }
        EvaluateUIState();
    }

    private void OnCardsLoaded()
    {
        RefreshAllCards();
    }

    private void OnCardAdded(CardAddedSignal signal)
    {
        _handCardContainer.AddCard(signal.Card);
    }

    private void OnCardRemoved(CardRemovedSignal signal)
    {
        _handCardContainer.RemoveCard(signal.CardId);
    }

    private void OnHandCardsChanged()
    {
        UpdateHandInfo();
    }

    private void OnFeatureLockChanged(FeatureLockChangedSignal signal)
    {
        if (signal.FeatureName == "HandUI")
        {
            _isHandUILocked = signal.IsLocked;
            EvaluateUIState();
        }
    }

    private void HandleVisualLayoutReady()
    {
        // 發送訊號，解除場景鎖，允許其他邏輯開始執行
        Debug.Log("[HandUIStateManager] 視覺排版已就緒，正式發送 SceneUIReadySignal");
        _signalBus.Fire<SceneUIReadySignal>();

        // 確保只發送一次初始化鎖，發送後即刻解除訂閱
        _handCardContainer.OnVisualLayoutReady -= HandleVisualLayoutReady;
    }
    #endregion


    #region Core Methods
    private void EvaluateUIState()
    {
        // 1. 評估是否有任何強制隱藏的條件
        bool shouldBeSubtle = _isSignalForcedSubtle || _isCameraForcedSubtle || _isDragForcedSubtle || _isHandUILocked;

        // 2. 計算目標可見度
        UIVisibilityHeadupMode targetVisibility = shouldBeSubtle
            ? UIVisibilityHeadupMode.Subtle
            : UIVisibilityHeadupMode.Standard;

        // 3. 計算目標排版 (如果是強制隱藏狀態，排版一律縮至右下角；否則依照玩家選擇的基礎模式)
        HandDisplayMode targetDisplayMode = shouldBeSubtle
            ? HandDisplayMode.BottomRight
            : _currentMode;

        // 4. 派發指令至 View 層 (HandCardContainer)
        if (_currentVisibility != targetVisibility)
        {
            _currentVisibility = targetVisibility;
            _handCardContainer.SetVisibilityMode(_currentVisibility, false);
        }

        _handCardContainer.SetHandMode(targetDisplayMode);
    }

    private void HandleDragSubtleMode()
    {
        // 1. 先取得系統當前是否允許輸入的狀態
        bool isCardInputEnabled = _cardInputManager.IsCardInputEnabled;

        // 判斷是否正在拖曳卡牌或棋子
        bool isDraggingCard = isCardInputEnabled &&
                          _cardInputManager.HasActiveCardInput &&
                          _cardInputManager.CollectInputData().IsDragging;

        bool isDraggingBoard = _boardInputService.GetCurrentState() == InputState.Dragging;
        bool isCurrentlyDragging = isDraggingCard || isDraggingBoard;

        // 如果開始拖曳，且尚未進入強制 Subtle 狀態
        if (isCurrentlyDragging != _isDragForcedSubtle)
        {
            if (isCurrentlyDragging && !_isDragForcedSubtle)
            {
                _preDragMode = _currentMode;     // 記住拖曳前的狀態
            }
            else if (!isCurrentlyDragging && _isDragForcedSubtle)
            {
                _currentMode = _preDragMode;    // 結束拖曳：還原玩家原本的模式
            }

            _isDragForcedSubtle = isCurrentlyDragging;
            EvaluateUIState();
        }
    }

    public void ToggleHandMode()
    {
        // 若處於強制狀態鎖定中，忽略玩家手動切換
        if (_isSignalForcedSubtle || _isCameraForcedSubtle || _isDragForcedSubtle || _isHandUILocked)
            return;

        _currentMode = (_currentMode == HandDisplayMode.Centered)
            ? HandDisplayMode.BottomRight
            : HandDisplayMode.Centered;

        EvaluateUIState(); 
    }

    public void RefreshAllCards()
    {
        var handCards = _handManager.GetHandCards();
        _handCardContainer.RefreshAllCards(handCards);
    }

    public void UpdateHandInfo()
    {
        int currentCount = _handManager.GetHandCards().Count;
        int maxCount = _handManager.GetMaxHandSize();
        Debug.Log($"手牌數量: {currentCount}/{maxCount}");
    }

    public void SetHandDisplayMode(HandDisplayMode mode)
    {
        if (_isSignalForcedSubtle || _isCameraForcedSubtle || _isDragForcedSubtle || _isHandUILocked)
            return;

        if (_currentMode == mode) return;

        _currentMode = mode;

        EvaluateUIState();
    }
    #endregion
}
