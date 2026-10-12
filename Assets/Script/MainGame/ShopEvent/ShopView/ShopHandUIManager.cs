using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class ShopHandUIManager : MonoBehaviour, IHandUIStateManager
{
    [Inject] private SignalBus _signalBus;
    [Inject] private IHandService _handManager;
    [Inject] private IHandCardContainer _handCardContainer;

    // 雙擊檢測變數
    [SerializeField] private float doubleClickTime = 0.3f; // 雙擊時間閾值（秒）
    private float lastClickTime = 0f;

    private bool _isInitialized = false;

    private HandDisplayMode _currentMode = HandDisplayMode.Centered;
    private UIVisibilityHeadupMode _currentVisibility = UIVisibilityHeadupMode.Standard;


    #region Initialize 
    void Awake()
    {
        Debug.Log($"[HandUIManager] Awake - Time: {Time.time}");
        _signalBus.Subscribe<CardsLoadedSignal>(OnCardsLoaded);
        _signalBus.Subscribe<CardAddedSignal>(OnCardAdded);
        _signalBus.Subscribe<CardRemovedSignal>(OnCardRemoved);
        _signalBus.Subscribe<HandCardsChangedSignal>(OnHandCardsChanged);
    }
    void OnDestroy()
    {
        _signalBus.Unsubscribe<CardsLoadedSignal>(OnCardsLoaded);
        _signalBus.Unsubscribe<CardAddedSignal>(OnCardAdded);
        _signalBus.Unsubscribe<CardRemovedSignal>(OnCardRemoved);
        _signalBus.Unsubscribe<HandCardsChangedSignal>(OnHandCardsChanged);
    }

    public void InitializeView()
    {
        RefreshAllCards();
        _handCardContainer.SetHandMode(_currentMode);
        _handCardContainer.SetVisibilityMode(_currentVisibility);
        _isInitialized = true;
    }
    #endregion

    #region Signal Handlers
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
    #endregion


    #region 業務邏輯
    private void Update()
    {
        if (!_isInitialized) return;

        HandleDoubleClick();
    }


    private void HandleDoubleClick()
    {
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

    public void ToggleHandMode()
    {
        _currentMode = (_currentMode == HandDisplayMode.Centered)
            ? HandDisplayMode.BottomRight 
            : HandDisplayMode.Centered;

         _handCardContainer.SetHandMode(_currentMode);
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
        if (_currentMode == mode) return;

        _currentMode = mode;

        // 透過 Zenject Signal 發送更新
        _signalBus.Fire(new ChangeHandUIVisibilitySignal
        {
            DisplayMode = mode,
            VisibilityMode = _currentVisibility // 帶入當前狀態
        });
    }
    #endregion
}

