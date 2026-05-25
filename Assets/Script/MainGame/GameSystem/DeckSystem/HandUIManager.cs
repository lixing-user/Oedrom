using System;
using UnityEngine;
using Zenject;

//Interfaces Layer
public interface IHandUIManager
{
    void RefreshAllCards();
    void UpdateHandInfo();
}

public enum HandDisplayMode { Centered, BottomRight }

public class HandUIManager : MonoBehaviour, IHandUIManager
{
    [Inject] private ITutorialOverrideService _tutorialOverride;

    [Inject] private SignalBus _signalBus;
    [Inject] private IHandService _handManager;
    [Inject] private IHandCardContainer _handCardContainer;

    [Inject] private IBoardInputService _boardInputService;
    [Inject] private ICardInputManager _cardInputManager;

    // 雙擊檢測變數
    [SerializeField] private float doubleClickTime = 0.3f; // 雙擊時間閾值（秒）
    private float lastClickTime = 0f;

    private bool _isInitialized = false;

    private HandDisplayMode currentMode = HandDisplayMode.Centered;
    private UIVisibilityHeadupMode _currentVisibility = UIVisibilityHeadupMode.Standard;

    private bool _isHandUILocked = false;
    private bool _isForcedSubtle = false;
    private HandDisplayMode _preDragMode = HandDisplayMode.Centered;

    #region Initialize 
    void Awake()
    {
        _signalBus.Subscribe<FeatureLockChangedSignal>(OnFeatureLockChanged);

        _signalBus.Subscribe<CameraModeChangedSignal>(OnCameraModeChanged);
        _signalBus.Subscribe<ToggleHandDeckSignal>(OnGameStarted);

        _signalBus.Subscribe<CardsLoadedSignal>(OnCardsLoaded);
        _signalBus.Subscribe<CardAddedSignal>(OnCardAdded);
        _signalBus.Subscribe<CardRemovedSignal>(OnCardRemoved);
        _signalBus.Subscribe<HandCardsChangedSignal>(OnHandCardsChanged);
    }
    void OnDestroy()
    {
        _signalBus.Unsubscribe<FeatureLockChangedSignal>(OnFeatureLockChanged);
        _signalBus.Unsubscribe<CameraModeChangedSignal>(OnCameraModeChanged);
        _signalBus.Unsubscribe<ToggleHandDeckSignal>(OnGameStarted);

        _signalBus.Unsubscribe<CardsLoadedSignal>(OnCardsLoaded);
        _signalBus.Unsubscribe<CardAddedSignal>(OnCardAdded);
        _signalBus.Unsubscribe<CardRemovedSignal>(OnCardRemoved);
        _signalBus.Unsubscribe<HandCardsChangedSignal>(OnHandCardsChanged);
    }

    private void Start()
    {
        RefreshAllCards();
        _handCardContainer.SetHandMode(HandDisplayMode.Centered);
        _handCardContainer.SetVisibilityMode(UIVisibilityHeadupMode.Subtle, immediate: true);
        _isInitialized = true;

        Debug.Log("[HandUIManager] UI 已準備就緒，發送 SceneUIReadySignal");
        _signalBus.Fire<SceneUIReadySignal>();
    }
    #endregion

    #region Signal Handlers
    private void OnGameStarted(ToggleHandDeckSignal signal)
    {
        ApplyHandMode(HandDisplayMode.Centered);
        ApplyVisibility(UIVisibilityHeadupMode.Standard);
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
                _isForcedSubtle = true;
                ApplyVisibility(UIVisibilityHeadupMode.Subtle);
                break;

            case CameraMode.Default:
            case CameraMode.TopView:
            case CameraMode.Uninitialized:
            default:
                // 回到一般模式時，解除強制並恢復為 Standard 模式
                _isForcedSubtle = false;
                ApplyVisibility(UIVisibilityHeadupMode.Standard);
                break;
        }
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

            if (_isHandUILocked)
            {
                // 當被鎖定時，強制進入 Subtle 且切換到右下角
                _isForcedSubtle = true;
                ApplyHandMode(HandDisplayMode.BottomRight);
                ApplyVisibility(UIVisibilityHeadupMode.Subtle);
            }
            else
            {
                // 解鎖時恢復
                _isForcedSubtle = false;
                ApplyHandMode(HandDisplayMode.Centered);
                ApplyVisibility(UIVisibilityHeadupMode.Standard);
            }
        }
    }
    #endregion


    #region 業務邏輯
    private void Update()
    {
        if (!_isInitialized) return;

        HandleDragSubtleMode();
        HandleDoubleClick();
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
        if (isCurrentlyDragging && !_isForcedSubtle)
        {
            _preDragMode = currentMode;                 // 記住拖曳前的狀態
            _isForcedSubtle = true;                     // 標記為強制狀態
            ApplyHandMode(HandDisplayMode.BottomRight); // 強制切換到右下角 (Subtle)
        }
        // 如果拖曳結束，且正在強制 Subtle 狀態中
        else if (!isCurrentlyDragging && _isForcedSubtle)
        {
            _isForcedSubtle = false;                    // 解除強制狀態
            ApplyHandMode(_preDragMode);                // 恢復拖曳前的狀態
        }
    }

    private void HandleDoubleClick()
    {
        if (_isForcedSubtle || _isHandUILocked) return;

        if (Input.GetMouseButtonDown(0))
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= doubleClickTime)
            {
                ToggleHandMode();
            }

            lastClickTime = Time.time;
        }
    }

    private void ToggleHandMode()
    {
        currentMode = (currentMode == HandDisplayMode.Centered)
            ? HandDisplayMode.BottomRight 
            : HandDisplayMode.Centered;

         _handCardContainer.SetHandMode(currentMode);
    }

    private void ApplyHandMode(HandDisplayMode mode)
    {
        currentMode = mode;
        _handCardContainer.SetHandMode(currentMode);
    }

    private void ApplyVisibility(UIVisibilityHeadupMode mode)
    {
        _currentVisibility = mode;
        _handCardContainer.SetVisibilityMode(_currentVisibility, false);
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
    #endregion
}

public class SceneUIReadySignal { }

public struct ToggleHandDeckSignal { }