using ChessPieceCardProduction;
using System.Linq;
using UnityEngine;
using Zenject;

public class ShopInputService : ICardInputService
{
    [Inject] private SignalBus _signalBus;
    [Inject] private IHandService _handManager;
    [Inject] private ICardVisualizerService _visualizer;
    [Inject] private ICardConfigProvider _cardConfigProvider;

    private CardID _selectedCardId = CardID.Empty;
    private CardState _currentState = CardState.Idle;

    private float _pointerDownTime = 0f;
    private float _longPressThreshold = 0.4f;

    public CardState CurrentState => _currentState;


    public void ProcessInput(InputData inputData)
    {
        if (inputData.Source == InputSource.Board3D && inputData.IsMouseUp)
        {
            if (!string.IsNullOrEmpty(inputData.HitShopItemId))
            {
                Debug.Log($"[InputService] 玩家點擊了 3D 商品: {inputData.HitShopItemId}");
                _signalBus.Fire(new ShopItemClickedSignal { ItemId = inputData.HitShopItemId });
                return;
            }
        }

        //if (inputData.Source == InputSource.CardUI || inputData.IsMouseDown || inputData.IsMouseUp)
        //{
        //    Debug.Log($"<color=#00FFFF>[ShopInputService 接收資料]</color> 來源:{inputData.Source} | " +
        //              $"當前狀態:{_currentState} | 按下:{inputData.IsMouseDown} | " +
        //              $"拖曳:{inputData.IsDragging} | 卡牌ID:{inputData.HitCard}");
        //}
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
            Debug.Log("[ShopInputService] 強制取消當前操作，重置狀態");
            TransitionToIdle();
        }
    }

    #region 狀態處理邏輯
    private void HandleIdleState(InputData inputData)
    {
        if (inputData.IsMouseDown && inputData.HitCard != null && !inputData.HitCard.IsEmpty)
        {
            var card = _handManager.GetHandCards().FirstOrDefault(c => c.CardId == inputData.HitCard);
            if (card == null) return;

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
            // 如果按住的時間超過閾值，正式進入拖曳狀態
            if (timePressedDown >= _longPressThreshold)
            {
                var card = _handManager.GetHandCards().FirstOrDefault(c => c.CardId == _selectedCardId);
                if (card != null)
                {
                    TransitionToDragging(inputData, card);
                }
                else
                {
                    TransitionToIdle();
                }
            }
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
            AttemptActionAndTransition(inputData);
        }
    }
    #endregion

    #region 狀態切換與結果執行
    private void AttemptActionAndTransition(InputData inputData)
    {
        if (inputData.HitLayer == LayerMask.NameToLayer("BattleTriggerArea"))
        {
            _signalBus.Fire(new RequestLootBattleSignal { CardId = _selectedCardId });
            _visualizer.PlayPlacementSuccessEffect();
            Debug.Log("成功拖曳至戰鬥區！");
        }
        else if (inputData.HitLayer == LayerMask.NameToLayer("SellTriggerArea"))
        {
            var card = _handManager.GetHandCards().FirstOrDefault(c => c.CardId == _selectedCardId);
            _signalBus.Fire(new RequestAddSellItemSignal { CardId = _selectedCardId, PieceType = card.CardType });
            _visualizer.PlayPlacementSuccessEffect();
            Debug.Log("成功拖曳至販售區！");
        }
        else if (inputData.HitLayer == LayerMask.NameToLayer("RepairTriggerArea"))
        {
            var card = _handManager.GetHandCards().FirstOrDefault(c => c.CardId == _selectedCardId);
            _signalBus.Fire(new RequestAddRepairItemSignal { CardId = _selectedCardId, PieceType = card.CardType });
            _visualizer.PlayPlacementSuccessEffect();
            Debug.Log("成功拖曳至修復區！");
        }
        else
        {
            _visualizer.PlayPlacementFailedEffect();
            Debug.Log("未放置在戰鬥區，取消拖曳");
        }

        TransitionToIdle();
    }

    private void TransitionToHover(InputData inputData, HandCard card)
    {
        _currentState = CardState.Hover;
        _pointerDownTime = inputData.Timestamp;
        _selectedCardId = card.CardId;
    }

    private void TransitionToDragging(InputData inputData, HandCard card)
    {
        _currentState = CardState.Dragging;

        // 向 Provider 拿取這張卡的 3D Prefab，並通知 Visualizer 生成
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
            Debug.LogError($"[ShopInput] 找不到卡牌配置: {card.CardType}");
            TransitionToIdle();
        }
    }

    private void TransitionToIdle()
    {
        _currentState = CardState.Idle;
        _selectedCardId = CardID.Empty;
        _pointerDownTime = 0f;

        // 銷毀 3D 預覽物件
        _visualizer.DestroyDragInstance();
    }
    #endregion
}

#region Signals
public class RequestLootBattleSignal
{
    public CardID CardId;
}

public class RequestAddSellItemSignal
{
    public CardID CardId;
    public ChessPieceType PieceType; // 傳遞給 3D View 生成對應模型用
}

public class RequestAddRepairItemSignal
{
    public CardID CardId;
    public ChessPieceType PieceType; // 傳遞棋子類型，方便 3D View 生成對應模型
}

public class ShopItemClickedSignal
{
    public string ItemId;
}
#endregion