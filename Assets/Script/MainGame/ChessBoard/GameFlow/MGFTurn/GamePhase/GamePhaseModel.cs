using ChessPieceCardProduction;
using System.Collections.Generic;
using UnityEngine;

//主狀態機
public enum GameFlow { Setup, PlayerTurn, EnemyTurn, Resolution, GameEnd}
public enum Team { None = -1, White = 0, Black = 1 }

//子狀態機（PlayerPhase 內部）
public enum GamePhaseState
{
    StandbyPhase,
    MainPhase1, //放置卡牌到棋盤、選擇卡牌、使用技能
    MovementPhase,  //移動棋子
    MainPhase2, //觸發隨機事件、攻擊敵方棋子
    ResolutionPhase,
    EndPhase
}

// Context Object pattern / Application
public class GamePhaseModel 
{
    // === StandbyPhase 相關數據 ===
    public bool IsResourceReplenished { get; set; }


    // === MainPhase1 \  MovementPhase 相關數據 ===
    public bool HasPlacedCardThisTurn { get; set; }
    public int PlacedCardCount { get; set; }
    public bool IsInFlexiblePhase { get; set; }
    public bool CanSwitchToMovement { get; set; }    // 是否允許切換到移動階段
    public bool CanSwitchToCardPlacement { get; set; } // 是否允許切換回卡牌階段
    public bool HasMovedThisTurn { get; set; }
    public List<PieceMovementRecord> MovementRecords { get; private set; } = new List<PieceMovementRecord>();
    public List<PendingPieceMove> PendingMoves { get; private set; } = new List<PendingPieceMove>();
    public BattleSessionData PendingBattle { get; set; }


    // === MainPhase2 相關數據 ===
    public ResolutionType CurrentResolutionType { get; set; }
    public Vector2Int? LastMovedPiecePosition { get; set; }
    public PieceID LastMovedPieceId { get; set; }
    public Vector2Int? TriggerPosition { get; set; }
    public bool IsBattleTriggered { get; set; }
    public bool IsRandomEventTriggered { get; set; }
    public int AttackCount { get; set; }


    // === ResolutionPhase 相關數據 ===
    public BattleResultData PendingBattleResult { get; set; }
    public RandomEventData PendingRandomEvent { get; set; } // 記錄抽到的事件
    public PieceID EventTriggerPieceId { get; set; }        // 記錄觸發事件的棋子
    public int SelectedEventOptionIndex { get; set; }


    // === 跨階段共享數據 ===
    public float RemainingSupply { get; set; }


    // === 階段完成條件 ===
    public bool CanProgressToNextPhase { get; set; }



    // 建構函數初始化預設值
    public GamePhaseModel()
    {
        ResetForNewTurn();
    }

    public void ResetForNewTurn()
    {
        IsResourceReplenished = false;
        PlacedCardCount = 0;
        HasPlacedCardThisTurn = false;
        IsInFlexiblePhase = true;
        CanSwitchToMovement = true;
        CanSwitchToCardPlacement = true;
        HasMovedThisTurn = false;
        IsBattleTriggered = false;
        IsRandomEventTriggered = false;
        PendingRandomEvent = null;
        EventTriggerPieceId = null;
        SelectedEventOptionIndex = -1;
        LastMovedPieceId = null;
        AttackCount = 0;
        CurrentResolutionType = ResolutionType.None;
        PendingBattle = null;
        PendingBattleResult = null;
        CanProgressToNextPhase = false;
        MovementRecords.Clear();
    }


    /// <summary>
    /// 添加移動記錄
    /// </summary>
    public void AddMovementRecord(PieceID pieceId, Vector2Int fromPosition, Vector2Int toPosition)
    {
        MovementRecords.Add(new PieceMovementRecord
        {
            PieceId = pieceId,
            FromPosition = fromPosition,
            ToPosition = toPosition,
            MovedAt = Time.time
        });

        Debug.Log($"[Model] 記錄移動: {pieceId} 從 {fromPosition} 到 {toPosition}");
    }

    /// <summary>
    /// 添加預覽棋子記錄
    /// </summary>
    public void AddPendingMove(PieceID pieceId, Vector2Int from, Vector2Int to)
    {
        PendingMoves.Clear();
        PendingMoves.Add(new PendingPieceMove { PieceId = pieceId, From = from, To = to });
        Debug.Log($"[Model] 新增預覽移動: {pieceId} {from} → {to}");
    }
    public void ClearPendingMoves()
    {
        PendingMoves.Clear();
    }
}