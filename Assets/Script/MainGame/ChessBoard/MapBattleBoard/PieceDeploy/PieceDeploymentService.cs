using ChessPieceCardProduction;
using System.Collections.Generic;
using System.Linq;
using Systems.SceneManagement;
using UnityEngine;
using Zenject;

public interface IPieceDeploymentService
{
    DeploymentResult AutoArrangePlayerPieces();
    ClearResult ClearPlayerPieces();
    ClearResult ClearPiecesAfterBattle(bool returnAsBroken);
    int GetDeployedPieceCount();
    int GetMaxDeploymentCount();
}


// Application/Services/PieceDeploymentService.cs
public class PieceDeploymentService : IPieceDeploymentService
{
    [Inject] private IGameBoard _board;
    [Inject] private BoardDimensions _dimensions;
    [Inject] private IDomainEventDispatcher _eventDispatcher;

    [Inject] private SignalBus _signalBus;
    [Inject] private IDeploymentValidator _validator;
    [Inject] private BattleContextHolder _battleContext;
    [Inject] private ICombatModifierService _modifierService;

    [Inject] private IHandService _handService;
    [Inject] private IResourceManager _resourceManager;
    [Inject] private IPieceSpawner _pieceSpawner;
    [Inject] private ICardSpawner _cardSpawner;

    #region 核心業務方法

    #region 自動佈陣
    public DeploymentResult AutoArrangePlayerPieces()
    {
        var result = new DeploymentResult
        {
            Success = false,
            PlacedCount = 0
        };

        // 1. 獲取手牌
        var handCards = _handService.GetHandCards();
        if (handCards.Count == 0)
        {
            result.ErrorType = DeploymentErrorType.EmptyHand;
            result.ErrorMessage = "手牌為空，無法佈陣";
            Debug.LogWarning("[PieceDeploymentService] " + result.ErrorMessage);
            return result;
        }

        // 2. 獲取可用位置（前兩排）
        var availableSlots = GetAvailableDeploymentSlots();
        if (availableSlots.Count == 0)
        {
            result.ErrorType = DeploymentErrorType.BoardFull;
            result.ErrorMessage = "棋盤佈陣區域已滿";
            Debug.LogWarning("[PieceDeploymentService] " + result.ErrorMessage);
            return result;
        }

        // 3. 計算士氣限制下的可用空位
        float currentMorale = _resourceManager.GetVariable(VariableType.morale);
        int maxAllowedPieces = Mathf.FloorToInt(currentMorale) + 5;
        int currentDeployedCount = GetDeployedPieceCount(); 
        int availableCapacity = Mathf.Max(0, maxAllowedPieces - currentDeployedCount);

        // 如果場上棋子已經達到或超過士氣上限
        if (availableCapacity <= 0)
        {
            result.ErrorType = DeploymentErrorType.InsufficientMorale;
            result.ErrorMessage = $"士氣限制：當前士氣({currentMorale})最多只能放置 {maxAllowedPieces} 枚棋子";
            Debug.LogWarning("[PieceDeploymentService] " + result.ErrorMessage);
            return result;
        }

        // 4. 逐個放置
        var sortedCards = SortCardsForDeployment(handCards);

        // 核心修改：在原本的 Mathf.Min 中加入 availableCapacity 作為限制
        int maxPlaceCount = Mathf.Min(sortedCards.Count, availableSlots.Count, availableCapacity);

        for (int i = 0; i < maxPlaceCount; i++)
        {
            var card = sortedCards[i];
            //var pos = availableSlots[i];

            // [核心修改] 根據棋子類型尋找最佳空位，而不是依序硬塞
            var bestPos = FindBestSlotForPiece(card.CardType, availableSlots);

            if (bestPos == null)
            {
                Debug.LogWarning($"[PieceDeploymentService] 找不到適合 {card.CardType} 的空位");
                continue;
            }

            // 4.1 使用 PieceSpawner 創建棋子（Domain + View）
            ChessPiece piece = _pieceSpawner.SpawnPieceFromCard(
                card.CardId,
                card.CardType,
                //pos,
                bestPos,
                Team.White
            );

            if (piece == null)
            {
                Debug.LogWarning($"[PieceDeploymentService] 創建棋子失敗: {card.CardType}");
                continue;
            }

            // [核心修改] 將已被佔用的位置從可用清單中移除
            availableSlots.Remove(bestPos);

            // 4.2 從手牌中移除
            _handService.RemoveCard(card.CardId, RemovalReason.Played);
            result.PlacedCount++;

            //Debug.Log($"[PieceDeploymentService] 佈陣: {card.CardType} at {pos}");
            Debug.Log($"[PieceDeploymentService] 佈陣: {card.CardType} at {bestPos}");
        }


        // 5. 觸發事件
        _eventDispatcher.Dispatch(_board.DomainEvents);
        _signalBus.Fire(new PiecesArrangedSignal(result.PlacedCount));

        result.Success = result.PlacedCount > 0;
        Debug.Log($"[PieceDeploymentService] 自動佈陣完成: {result.PlacedCount} 個棋子");

        return result;
    }
    private List<HandCard> SortCardsForDeployment(IReadOnlyList<HandCard> handCards)
    {
        var sorted = new List<HandCard>();

        // King 優先
        var kingCard = handCards.FirstOrDefault(c => c.CardType == ChessPieceType.King);
        if (kingCard != null)
        {
            sorted.Add(kingCard);
        }

        // 其餘卡牌按類型順序
        var remainingCards = handCards
            .Where(c => c.CardType != ChessPieceType.King)
            .OrderBy(c => c.CardType)
            .ToList();

        sorted.AddRange(remainingCards);
        return sorted;
    }
    #endregion

    #region 清除玩家棋子
    public ClearResult ClearPlayerPieces()
    {
        // 1. 找到所有白方棋子（排除 King）
        var piecesToClear = GetPlayerPiecesForClear();
        if (piecesToClear.Count == 0)
        {
            Debug.Log("[PieceDeploymentService] 沒有可清除的棋子");
            return new ClearResult { Success = true, ClearedCount = 0 };
        }

        return ExecuteClearingProcess(piecesToClear, false);
    }

    /// <summary>
    /// 獲取可清除的玩家棋子（排除當前 King）
    /// </summary>
    private List<ChessPiece> GetPlayerPiecesForClear()
    {
        var pieces = new List<ChessPiece>();

        // 遍歷前兩排
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < _dimensions.BoardSizeX; x++)
            {
                var pos = new PiecePosition(x, y);
                var piece = _board.GetPieceAt(pos);

                if (piece != null && piece.Team == Team.White)
                {
                    if (!piece.IsLocked)
                    {
                        pieces.Add(piece);
                    }
                    else
                    {
                        Debug.Log($"[DeploymentService] 跳過鎖定棋子: {piece.Type}");
                    }
                }
            }
        }

        return pieces;
    }

    #region 戰鬥結束清理棋盤
    public ClearResult ClearPiecesAfterBattle(bool returnAsBroken)
    {
        var piecesToClear = GetPiecesToClearAfterBattle();

        if (piecesToClear.Count == 0)
        {
            return new ClearResult { Success = true, ClearedCount = 0 };
        }

        return ExecuteClearingProcess(piecesToClear, returnAsBroken);
    }

    private List<ChessPiece> GetPiecesToClearAfterBattle()
    {
        var pieces = new List<ChessPiece>();
        var pendingBattle = _battleContext.CurrentBattle;

        for (int y = 0; y < _dimensions.BoardSizeY; y++)
        {
            for (int x = 0; x < _dimensions.BoardSizeX; x++)
            {
                var piece = _board.GetPieceAt(new PiecePosition(x, y));

                if (piece != null && piece.Team == Team.White)
                {
                    // [核心邏輯] 如果這個棋子是當前戰鬥的攻擊方或防守方，則跳過不回收
                    if (pendingBattle != null &&
                       (piece.Id == pendingBattle.AttackerId || piece.Id == pendingBattle.DefenderId))
                    {
                        Debug.Log($"[DeploymentService] 保留戰鬥結算棋子不回收: {piece.Type}");
                        continue;
                    }

                    pieces.Add(piece);
                }
            }
        }
        return pieces;
    }
    #endregion

    #region 共用核心邏輯
    private ClearResult ExecuteClearingProcess(List<ChessPiece> piecesToClear, bool returnAsBroken)
    {
        var result = new ClearResult { Success = false, ClearedCount = 0 };

        foreach (var piece in piecesToClear)
        {
            HandCard card = _cardSpawner.SpawnCardFromPiece(piece, GameMode.Mode3D);
            if (card == null) continue;

            // 若為戰敗結算，賦予破損狀態
            if (returnAsBroken)
            {
                card.SetBroken();
            }

            _handService.AddCard(card);
            _pieceSpawner.RemovePiece(piece.Id);
            _board.RemovePiece(piece.CurrentPosition);

            result.ClearedCount++;
            Debug.Log($"[PieceDeploymentService] 回收棋子: {piece.Type} → 卡牌 {card.CardId} (Broken: {returnAsBroken})");
        }

        _eventDispatcher.Dispatch(_board.DomainEvents);
        _signalBus.Fire(new PiecesClearedSignal(result.ClearedCount));

        result.Success = result.ClearedCount > 0;
        return result;
    }
    #endregion

    #endregion

    #endregion

    #region 輔助方法
    private PiecePosition FindBestSlotForPiece(ChessPieceType pieceType, List<PiecePosition> availableSlots)
    {
        if (availableSlots.Count == 0) return null;

        PiecePosition bestSlot = null;

        // 判斷是否為前排作戰單位
        if (pieceType == ChessPieceType.Pawn)
        {
            // Pawn 傾向前排 (y >= 1)
            // 排序邏輯：優先找 Y 越大的位置（越靠近前線），若 Y 相同則盡量排在中間或依序排列
            bestSlot = availableSlots
                .OrderByDescending(p => p.Y) // 優先選 Y 最大的 (最前排)
                .ThenBy(p => p.X)            // 然後從左到右
                .FirstOrDefault();
        }
        else
        {
            // Rook, Bishop, Knight, King, Queen 傾向後排 (y = 0)
            // 排序邏輯：優先找 Y 越小的位置（越靠近底線）
            bestSlot = availableSlots
                .OrderBy(p => p.Y)           // 優先選 Y 最小的 (y=0)
                .ThenBy(p => p.X)            // 然後從左到右
                .FirstOrDefault();
        }

        return bestSlot;
    }

    private List<PiecePosition> GetAvailableDeploymentSlots()
    {
        var slots = new List<PiecePosition>();
        int maxDeployY = _modifierService.IsDeployZoneExpanded() ? 4 : 2;

        for (int y = 0; y < maxDeployY; y++)
        {
            for (int x = 0; x < _dimensions.BoardSizeX; x++)
            {
                var pos = new PiecePosition(x, y);
                if (_board.GetPieceAt(pos) == null)
                {
                    slots.Add(pos);
                }
            }
        }

        return slots;
    }

    /// <summary>
    /// 獲取當前佈陣的棋子數量
    /// </summary>
    public int GetDeployedPieceCount()
    {
        return _validator.GetDeployedPieceCount();
    }

    public int GetMaxDeploymentCount()
    {
        return _validator.GetMaxDeploymentCount();
    }
    #endregion
}


#region Signals
public class PiecesArrangedSignal
{
    public int PlacedCount { get; }

    public PiecesArrangedSignal(int placedCount)
    {
        PlacedCount = placedCount;
    }
}

public class PiecesClearedSignal
{
    public int ClearedCount { get; }

    public PiecesClearedSignal(int clearedCount)
    {
        ClearedCount = clearedCount;
    }
}
#endregion