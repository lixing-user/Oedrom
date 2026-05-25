using ChessPieceCardProduction;
using System;
using System.Linq;
using UnityEngine;
using Zenject;


public interface ICardPlacementHandler
{
    void SetPlacementData(CardID cardId, ChessPieceType type, Team team);
    bool TryPlaceCard(InputData inputData);
}


// Application/Card/CardPlacementHandler.cs
public class CardPlacementHandler : ICardPlacementHandler
{
    //Domain
    //[Inject] private GameBoard<Tile> _gameBoard;
    [Inject] private IGameBoard _gameBoard;

    //Application
    [Inject] private IHandService _handService;
    [Inject] private IDomainEventDispatcher _eventDispatcher;
    [Inject] private IPieceSpawner _pieceSpawner;
    [Inject] private ICardPlacementRule _placementRule;

    //Infrastructure
    [Inject] private ICoordinateConverter _coordinateConverter;

    private CardID _pendingCardId;
    private ChessPieceType _pendingPieceType;
    private Team _pendingTeam;


    public void SetPlacementData(CardID cardId, ChessPieceType type, Team team)
    {
        _pendingCardId = cardId;
        _pendingPieceType = type;
        _pendingTeam = team;
    }

    public bool TryPlaceCard(InputData inputData)
    {
        // 1. 驗證輸入來源（卡牌放置只能從 CardUI 觸發）
        if (inputData.Source != InputSource.CardUI)
        {
            Debug.LogWarning("放置必須從卡牌拖曳觸發");
            return false;
        }
        if (!inputData.IsOverBoard)
        {
            Debug.LogWarning("卡牌必須放置在棋盤上");
            return false;
        }


        // 2. 將世界坐標轉換為棋盤坐標
        PiecePosition boardPosition = _coordinateConverter.WorldToPiecePosition(
            inputData.MouseWorldPosition
        );
        if (!boardPosition.IsValid(_gameBoard.Dimensions))
        {
            Debug.LogWarning("放置位置超出棋盤範圍");
            return false;
        }


        // 3. 驗證手牌
        var card = _handService.GetHandCards()
            .FirstOrDefault(c => c.CardId == _pendingCardId);
        if (card == null)
        {
            Debug.LogError($"卡牌 {_pendingCardId} 不在手牌中");
            return false;
        }


        // 4. 檢查放置規則（可選：檢查是否擊中有效 Tile）
        if (inputData.HitTilePosition == null)
        {
            Debug.LogWarning("未擊中有效的棋盤格子");
            return false;
        }

        Vector2Int hitPos = new Vector2Int(inputData.HitTilePosition.X, inputData.HitTilePosition.Y);
        if (!_placementRule.IsValidPlacement(hitPos))
        {
            Debug.LogWarning($"卡牌放置失敗：不符合當前場景的放置規則 ({hitPos})");
            return false;
        }


        // 5. 創建棋子並放置
        try
        {
            ChessPiece piece = _pieceSpawner.SpawnPieceFromCard(
                _pendingCardId,
                _pendingPieceType,
                boardPosition,
                _pendingTeam
            );

            if (piece == null)
            {
                Debug.LogWarning("棋子創建失敗");
                return false;
            }

            // 加入棋盤（觸發領域事件）
            _eventDispatcher.Dispatch(_gameBoard.DomainEvents);
            _gameBoard.ClearEvents();

            // 移除手牌
            _handService.RemoveCard(_pendingCardId, RemovalReason.Played);

            Debug.Log($"成功放置卡牌 {_pendingCardId} 到 {boardPosition}");
            return true;
        }
        catch (InvalidOperationException ex)
        {
            Debug.LogWarning($"放置失敗: {ex.Message}");
            return false;
        }
    }
}