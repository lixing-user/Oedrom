using ChessPieceCardProduction;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Systems.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

//Interfaces Layer
public class HandCardContainer : MonoBehaviour, IHandCardContainer
{
    [Inject] private ICardSpawner _cardSpawner;

    [SerializeField] public GameObject handCardDeck;
    [SerializeField] private HorizontalLayoutGroup layoutGroup;
    [SerializeField] public bool use2DUI = true;

    [Header("Dynamic Spacing Settings")]
    [SerializeField] private float cardWidth = 100f;
    [SerializeField] private int maxUncompressedCards = 6;

    [Header("Visibility Settings")]
    [SerializeField] private float _yStandard = 0f;
    [SerializeField] private float _ySubtle = -125f;
    [SerializeField] private float _transitionDuration = 0.3f;

    private RectTransform handCardDeckRect;
    private Canvas _parentCanvas;
    private Camera _lastRenderCamera;

    private float _currentDefaultSpacing;   // 紀錄當前模式的預設參數，供動態計算使用

    private Tweener _moveTween;


    private Dictionary<CardID, CardFacade> _cardViews = new();

    private void Awake()
    {
        if (handCardDeck == null)
            handCardDeck = GameObject.Find("HandCardDeck");
        if (layoutGroup == null)
            layoutGroup = handCardDeck.GetComponent<HorizontalLayoutGroup>();
        if (handCardDeck != null)
            handCardDeckRect = handCardDeck.GetComponent<RectTransform>();

        _parentCanvas = GetComponentInParent<Canvas>();
    }
    private void Start()
    {
        UpdateDynamicSpacing();
    }

    #region Card Management
    public void AddCard(HandCard handCard)
    {
        if (_cardViews.ContainsKey(handCard.CardId))
        {
            Debug.LogWarning($"卡牌 {handCard.CardId} 已存在於 UI 中");
            return;
        }

        GameMode mode = use2DUI ? GameMode.Mode2D : GameMode.Mode3D;

        CardFacade cardFacade = _cardSpawner.SpawnCardView(handCard, mode);
        
        cardFacade.transform.SetParent(handCardDeck.transform, false);

        _cardViews[handCard.CardId] = cardFacade;

        //UpdateCardPositions();
    }

    public void RemoveCard(CardID cardId)
    {
        if (_cardViews.TryGetValue(cardId, out CardFacade cardFacade))
        {
            _cardViews.Remove(cardId);
            Destroy(cardFacade.gameObject);
            //UpdateCardPositions();
        }
    }

    public void RefreshAllCards(IReadOnlyList<HandCard> handCards)
    {
        ClearAllCards();
        GameMode mode = use2DUI ? GameMode.Mode2D : GameMode.Mode3D;

        foreach (var handCard in handCards) //完整刷新（接收 Application 層數據）
        {
            AddCard(handCard);
        }

        //UpdateCardPositions();
    }

    public void ClearAllCards()
    {
        foreach (var cardFacade in _cardViews.Values)
        {
            if (cardFacade != null)
                Destroy(cardFacade.gameObject);
        }
        _cardViews.Clear();
    }
    #endregion


    #region Query Methods
    public CardFacade GetCardFacade(CardID cardId)
    {
        _cardViews.TryGetValue(cardId, out var cardFacade);
        return cardFacade;
    }

    public IEnumerable<CardFacade> GetAllCardFacades()
    {
        return _cardViews.Values;
    }
    #endregion


    #region Layout Management
    public void SetHandMode(HandDisplayMode mode)
    {
        RectTransform rt = handCardDeck.transform as RectTransform;

        Vector2 anchoredPos_Centered = Vector2.zero;
        Vector3 localScale_Centered = Vector3.one;
        float spacing_Centered = 95f;
        RectOffset padding_Centered = new RectOffset(-220, 0, 0, -350);

        Vector2 anchoredPos_BottomRight = Vector2.zero;
        Vector3 localScale_BottomRight = new Vector3(0.65f, 0.65f, 0.65f); 
        float spacing_BottomRight = 70f;
        RectOffset padding_BottomRight = new RectOffset(-715, 0, 0, -20);


        switch (mode)
        {
            case HandDisplayMode.Centered:
                handCardDeckRect.anchorMin = new Vector2(0.5f, 0.5f);
                handCardDeckRect.anchorMax = new Vector2(0.5f, 0.5f);
                handCardDeckRect.pivot = new Vector2(0.5f, 0.5f);
                handCardDeckRect.anchoredPosition = anchoredPos_Centered;
                handCardDeckRect.localScale = localScale_Centered;
                layoutGroup.childAlignment = TextAnchor.MiddleCenter;
                layoutGroup.spacing = spacing_Centered;
                layoutGroup.padding = padding_Centered;
                _currentDefaultSpacing = 95f;
                break;

            case HandDisplayMode.BottomRight:
                handCardDeckRect.anchorMin = new Vector2(1f, 0f);
                handCardDeckRect.anchorMax = new Vector2(1f, 0f);
                handCardDeckRect.pivot = new Vector2(1f, 0f);
                handCardDeckRect.anchoredPosition = anchoredPos_BottomRight;
                handCardDeckRect.localScale = localScale_BottomRight;
                layoutGroup.childAlignment = TextAnchor.LowerCenter;
                layoutGroup.spacing = spacing_BottomRight;
                layoutGroup.padding = padding_BottomRight;
                _currentDefaultSpacing = 70f;
                break;
        }

        UpdateDynamicSpacing();
    }

    private void UpdateDynamicSpacing()
    {
        if (layoutGroup == null) return;

        int cardCount = _cardViews.Count;

        // 1. 如果手牌數量 <= 6 張（maxUncompressedCards），直接使用預設原始間距
        if (cardCount <= maxUncompressedCards)
        {
            layoutGroup.spacing = _currentDefaultSpacing;
            StartCoroutine(RefreshLayoutDelayed());
            return;
        }

        // 2. 超過 6 張，計算出「6張卡牌原本會佔用的最大完美寬度」
        float targetSpacingRatio = 1f;
        if (cardCount <= 8)
        {
            // 6 到 8 張之間的過渡
            // 當 N=6 時 t=0 (比例為 95/95 = 1.0)
            // 當 N=8 時 t=1 (比例為 65/95 )
            float t = (cardCount - 6f) / (8f - 6f);
            float ratioAt8 = 65f / 95f;
            targetSpacingRatio = Mathf.Lerp(1f, ratioAt8, t);
        }
        else if (cardCount <= 10)
        {
            // 8 到 10 張之間的過渡
            // 當 N=8 時 t=0 (比例為 65/95)
            // 當 N=10 時 t=1 (比例為 53.5/95 )
            float t = (cardCount - 8f) / (10f - 8f);
            float ratioAt8 = 65f / 95f;
            float ratioAt10 = 53.5f / 95f;
            targetSpacingRatio = Mathf.Lerp(ratioAt8, ratioAt10, t);
        }
        else
        {
            // 10 張以上的極限防禦處理 (避免間距變成負數導致卡牌完全重疊)
            // 這裡設定每多 2 張，間距比例繼續平緩下降，極限最低設為 40/95
            float t = (cardCount - 10f) / 4f; // 假設 14 張達到極限壓縮
            float ratioAt10 = 53.5f / 95f;
            float minRatioLimit = 40f / 95f;
            targetSpacingRatio = Mathf.Lerp(ratioAt10, minRatioLimit, Mathf.Clamp01(t));
        }

        // 3. 把剩下的寬度平均分配給所有間隙 (這時算出來會是較小的正數，甚至負數)
        layoutGroup.spacing = _currentDefaultSpacing * targetSpacingRatio;

        StartCoroutine(RefreshLayoutDelayed());
    }

    private void LateUpdate()
    {
        // 檢測 Canvas 攝影機是否變更
        if (_parentCanvas != null && _parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Camera currentCamera = _parentCanvas.worldCamera;
            if (currentCamera != _lastRenderCamera)
            {
                _lastRenderCamera = currentCamera;
                OnCameraChanged();
            }
        }
    }

    private void OnCameraChanged()
    {
        Debug.Log("Canvas render camera changed, refreshing layout");
        StartCoroutine(RefreshLayoutDelayed());
    }

    private IEnumerator RefreshLayoutDelayed()
    {
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(handCardDeckRect);

        // 如果仍使用手動位置計算，取消 HorizontalLayoutGroup
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
            layoutGroup.enabled = true;
        }
    }

    public void SetVisibilityMode(UIVisibilityHeadupMode mode, bool immediate = false)
    {
        float targetY = (mode == UIVisibilityHeadupMode.Standard) ? _yStandard : _ySubtle;

        _moveTween?.Kill();

        if (immediate)
        {
            handCardDeckRect.anchoredPosition = new Vector2(handCardDeckRect.anchoredPosition.x, targetY);
        }
        else
        {
            // 僅針對 Y 軸進行 DOTween 動畫
            _moveTween = handCardDeckRect.DOAnchorPosY(targetY, _transitionDuration)
                                         .SetEase(Ease.OutCubic);
        }
    }
    #endregion
}

public interface IHandCardContainer
{
    void AddCard(HandCard handCard);
    void RemoveCard(CardID cardId);
    void RefreshAllCards(IReadOnlyList<HandCard> handCards);
    void ClearAllCards();

    void SetHandMode(HandDisplayMode mode);
    void SetVisibilityMode(UIVisibilityHeadupMode mode, bool immediate = false);

    CardFacade GetCardFacade(CardID cardId);
    IEnumerable<CardFacade> GetAllCardFacades();
}