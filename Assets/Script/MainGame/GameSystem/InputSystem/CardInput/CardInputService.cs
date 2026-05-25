using ChessPieceCardProduction;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

//Application Layer
public class CardInputService : ICardInputService
{
    [InjectOptional] private readonly List<ICardSelectionExtension> _selectionExtensions;

    [Inject] private ICardVisualizerService _visualizer;
    [Inject] private ICardPlacementHandler _placementHandler;
    [Inject] private ICardPlacementRule _placementRule;

    [Inject] private IBoardVisualizer _boardVisualizer;
    [Inject] private IHandService _handManager;
    [Inject] private IHandUIManager _handUIManager;
    [Inject] private ICardConfigProvider _cardConfigProvider;

    private CardState _currentState = CardState.Idle;
    private CardID _selectedCardId = CardID.Empty;
    private float _pointerDownTime = 0f;
    private float _longPressThreshold = 0.4f;

    public CardState CurrentState => _currentState;

    public void ProcessInput(InputData inputData)
    {
        switch (_currentState)
        {
            case CardState.Idle:
                HandleIdleState(inputData);
                break;
            case CardState.Hover:
                HandleHoverState(inputData);
                break;
            case CardState.Dragging:
                HandleDragState(inputData);
                break;
        }
    }

    public void ForceCancel()
    {
        if (_currentState != CardState.Idle)
        {
            Debug.Log("[CardInput] 強制取消選擇");
            TransitionToIdle();
        }
    }

    #region State Handlers
    private void HandleIdleState(InputData inputData)
    {
        if (inputData.IsMouseDown && inputData.HitCard != null && !inputData.HitCard.IsEmpty)
        {
            var card = _handManager.GetHandCards()
                           .FirstOrDefault(c => c.CardId == inputData.HitCard);

            if (card == null) return;

            // === 關鍵攔截邏輯 ===
            if (_selectionExtensions != null)
            {
                foreach (var extension in _selectionExtensions)
                {
                    if (!extension.CanSelectCard(card))
                    {
                        // 攔截成功，播放失敗回饋並中斷選取流程
                        _visualizer.PlayPlacementFailedEffect();
                        return;
                    }
                }
            }

            if (card.GetStatus() == CardStatus.broken)
            {
                _visualizer.ShowBrokenCardWarning();
                return;
            }

            TransitionToHover(inputData, card);
        }
    }

    private void HandleHoverState(InputData inputData)
    {
        float timePressedDown = Time.time - _pointerDownTime;

        if (inputData.IsDragging)
        {
            if (timePressedDown >= _longPressThreshold)
            {
                var card = _handManager.GetHandCards()
                    .FirstOrDefault(c => c.CardId == _selectedCardId);

                if (card != null)
                    TransitionToDragging(inputData, card);
                else
                    TransitionToIdle();
            }
        }
        else if (inputData.IsMouseUp && timePressedDown < _longPressThreshold)
        {
            Debug.Log("[CardInput] 短按取消選擇");
            TransitionToIdle();
        }
    }

    private void HandleDragState(InputData inputData)
    {
        if (inputData.IsDragging)
        {
            _visualizer.UpdateDragPosition(inputData.MouseWorldPosition);
        }
        else if (inputData.IsMouseUp)
        {
            var card = _handManager.GetHandCards()
               .FirstOrDefault(c => c.CardId == _selectedCardId);

            if (card != null)
                AttemptPlacementAndTransition(inputData, card);
            else
                TransitionToIdle();
        }
    }
    #endregion

    #region State Transitions
    private void AttemptPlacementAndTransition(InputData inputData, HandCard card)
    {
        _placementHandler.SetPlacementData(
            card.CardId,
            card.CardType,
            card.Team
        );

        bool success = _placementHandler.TryPlaceCard(inputData);

        if (success)
        {
            _visualizer.PlayPlacementSuccessEffect();
            _handUIManager.RefreshAllCards();
        }
        else
        {
            _visualizer.PlayPlacementFailedEffect();
        }

        TransitionToIdle();
    }

    private void TransitionToHover(InputData inputData, HandCard card)
    {
        _currentState = CardState.Hover;
        _pointerDownTime = inputData.Timestamp;
        _selectedCardId = card.CardId;
        //PieceInfoPanel.Instance.Show(cardData);
    }

    private void TransitionToDragging(InputData inputData, HandCard card)
    {
        _currentState = CardState.Dragging;

        if (_cardConfigProvider.TryGetConfig(card.CardType, out CardConfig config))
        {
            _visualizer.CreateDragInstance(
                card.CardType,
                config.previewPrefab,
                inputData.MouseWorldPosition,
                card.Team
            );
        }
        else
        {
            Debug.LogError($"找不到卡牌配置: {card.CardType}");
            TransitionToIdle();
        }

        var placementTiles = _placementRule.GetValidPlacementTiles();
        _boardVisualizer.HighlightTiles(placementTiles);
    }


    private void TransitionToIdle()
    {
        _currentState = CardState.Idle;
        _selectedCardId = CardID.Empty; 
        _visualizer.DestroyDragInstance();
        _boardVisualizer.RemoveHighlightTiles();
        _pointerDownTime = 0f;
    }
    #endregion
}

public interface ICardInputService
{
    void ProcessInput(InputData inputData);
    CardState CurrentState { get; }
    void ForceCancel();
}