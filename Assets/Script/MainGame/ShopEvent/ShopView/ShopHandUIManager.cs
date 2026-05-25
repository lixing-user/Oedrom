using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class ShopHandUIManager : MonoBehaviour, IHandUIManager
{
    [Inject] private SignalBus _signalBus;
    [Inject] private IHandService _handManager;
    [Inject] private IHandCardContainer _handCardContainer;

    // 雙擊檢測變數
    [SerializeField] private float doubleClickTime = 0.3f; // 雙擊時間閾值（秒）
    private float lastClickTime = 0f;

    private bool _isInitialized = false;

    private HandDisplayMode currentMode = HandDisplayMode.Centered;
    private UIVisibilityHeadupMode currentVisibility = UIVisibilityHeadupMode.Standard;


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
        _handCardContainer.SetHandMode(currentMode);
        _handCardContainer.SetVisibilityMode(currentVisibility);
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

