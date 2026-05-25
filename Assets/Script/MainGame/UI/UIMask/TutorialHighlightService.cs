using ChessPieceCardProduction;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TutorialHighlightService : MonoBehaviour 
{
    [Inject] private readonly ICoordinateConverter _coordinateConverter;
    [InjectOptional] private readonly IHandCardContainer _handCardContainer;

    private TutorialMaskView _view;
    private Camera _mainCamera;


    private void Start()
    {
        _view = GetComponent<TutorialMaskView>();
        _mainCamera = Camera.main;

        _view.Hide();
    }

    #region Hihglight Methods
    // 由 Action 呼叫：高光 3D 棋盤的格子
    public void HighlightBoardTile(PiecePosition position)
    {
        Vector3 worldPos = _coordinateConverter.PieceToWorldPosition(position);
        Vector2 screenPos = _mainCamera.WorldToScreenPoint(worldPos);

        // 假設一個格子的遮罩大小是 150x150
        _view.FocusOnScreenPosition(screenPos, new Vector2(150, 150));
    }

    // 由 Action 呼叫：高光手牌
    public void HighlightHandCard(CardID cardId)
    {
        // 1. 透過 Container 獲取對應的 CardFacade
        CardFacade cardFacade = _handCardContainer?.GetCardFacade(cardId);

        if (cardFacade != null)
        {
            // 2. 將 MonoBehaviour 的 transform 轉型為 RectTransform
            RectTransform cardRect = cardFacade.transform as RectTransform;

            if (cardRect != null)
            {
                // 3. 傳遞給 View 進行高光跟隨
                _view.FocusOnUIElement(cardRect);
            }
        }
        else
        {
            Debug.LogWarning($"[TutorialHighlightService] 找不到 ID 為 {cardId} 的卡牌 UI");
        }
    }

    public void HighlightAnchoredUI(Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        _view.FocusOnAnchoredPosition(anchorMin, anchorMax, anchoredPos, size);
    }

    public void HighlightUIElement(RectTransform targetUI)
    {
        if (targetUI != null)
        {
            _view.FocusOnUIElement(targetUI);
        }
    }

    public void ClearAllHighlights() => _view.Hide();
    #endregion
}
