using ChessPieceCardProduction;
using System;
using System.Collections.Generic;
using System.Linq;

// Domain : entity
public class ChessPiece
{
    #region 屬性&資料
    public PieceID Id { get; }
    public CardID SourceCardId { get; }

    public ChessPieceType Type { get; }
    public Team Team { get; private set; }
    public Team OriginalTeam { get; }   // 不可變的原始陣營屬性

    public PiecePosition CurrentPosition { get; private set; }
    public PieceState State { get; private set; }

    public bool HasMoved { get; private set; }
    public bool IsCaptured { get; private set; }
    public bool IsLocked { get; private set; }
    public bool IsBroken { get; private set; }
    public bool IsReversed { get; private set; } // 用於將棋模式，表示棋子已被翻轉


    public IMovementStrategy MovementStrategy { get; private set; }


    // 屬性：儲存當前所有狀態
    private readonly List<IStatusEffect> _activeStatuses = new();
    public IReadOnlyList<IStatusEffect> ActiveStatuses => _activeStatuses.AsReadOnly();
    #endregion\

    #region 業務邏輯
    public ChessPiece(
        PieceID id, 
        CardID sourceCardId,
        ChessPieceType type, 
        Team team, 
        PiecePosition startPosition, 
        IMovementStrategy strategy,
        PieceState initialState = PieceState.OnBoard,
        bool isRoyal = false)
    {
        Id = id;
        SourceCardId = sourceCardId;
        Type = type;
        Team = team;
        OriginalTeam = team;
        CurrentPosition = startPosition;
        HasMoved = false;
        IsCaptured = false;
        State = initialState;
        MovementStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }


    // 業務邏輯：移動
    public void MoveTo(PiecePosition newPosition)
    {
        if (IsCaptured)
            throw new InvalidOperationException("已被吃的棋子無法移動");
        if (State != PieceState.OnBoard)
            throw new InvalidOperationException($"只有在棋盤上的棋子可以移動，當前狀態：{State}");
        if (IsLocked)
            throw new InvalidOperationException("上鎖的棋子無法移動");

        CurrentPosition = newPosition;
        HasMoved = true;
    }

    // 業務邏輯：被吃
    public void Capture()
    {
        if (IsLocked)
            throw new InvalidOperationException("上鎖的棋子無法被吃");

        IsCaptured = true;
        IsReversed = false; // 被吃時，將棋子翻回正面
        State = PieceState.InStandby;
        CurrentPosition = null; 
    }

    //業務邏輯：轉換陣營 (僅修改 Team，不修改 OriginalTeam)
    public void ConvertToTeam(Team newTeam)
    {
        if (Team == newTeam) return;

        Team = newTeam;
    }

    public void ChangeState(PieceState newState)
    {
        State = newState;
    }

    // 業務邏輯：翻轉棋子方向
    public void ReverseDirection()
    {
        IsReversed = !IsReversed;
    }

    #region 擴充業務邏輯：重新放置
    public void Capture(CaptureMode mode)
    {
        if (IsLocked)
            throw new InvalidOperationException("上鎖的棋子無法被吃");

        IsCaptured = true;
        IsReversed = false; // 被吃時，將棋子翻回正面
        State = mode == CaptureMode.ToStandby
            ? PieceState.InStandby
            : PieceState.Removed;
        CurrentPosition = null;
    }

    // 業務邏輯：從俘虜區恢復
    public void RestoreFromCapture(PiecePosition newPosition)
    {
        if (State != PieceState.InStandby)
            throw new InvalidOperationException("只有被俘虜的棋子可以恢復");

        State = PieceState.OnBoard;
        IsCaptured = false;
        CurrentPosition = newPosition;
        HasMoved = true;
    }
    #endregion

    #region 擴充業務邏輯：上鎖機制
    public void Lock()
    {
        IsLocked = true;
        //Debug.Log($"[ChessPiece] {Type} 已鎖定");
    }
    public void Unlock()
    {
        IsLocked = false;
        //Debug.Log($"[ChessPiece] {Type} 已解鎖");
    }
    #endregion

    #region 擴充業務邏輯：組合移動模式
    public void SetMovementStrategy(IMovementStrategy strategy)
    {
        MovementStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }
    #endregion

    #region 擴充業務邏輯：事件狀態效果
    // 方法：新增與移除狀態
    public void AddStatus(IStatusEffect status)
    {
        _activeStatuses.Add(status);
        status.OnApply(this);
    }

    public void RemoveStatus(IStatusEffect status)
    {
        status.OnRemove(this);
        _activeStatuses.Remove(status);
    }

    // 方法：每回合結算狀態效果
    public void TickStatuses()
    {
        if (_activeStatuses.Count == 0) return;

        // 建立副本以進行遍歷，避免在迴圈中移除元素報錯
        var statusesToTick = _activeStatuses.ToList();

        foreach (var status in statusesToTick)
        {
            // 執行狀態的結算效果（例如：扣血），並檢查是否過期
            bool isExpired = status.OnTurnTick(this);

            if (isExpired)
            {
                RemoveStatus(status);
                // Debug.Log($"[ChessPiece] {Type} 的狀態 {status.Name} 已解除");
            }
        }
    }
    #endregion

    // 業務查詢：是否為敵方
    public bool IsEnemyOf(ChessPiece other)
    {
        return Team != other.Team;
    }
    #endregion
}

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public enum PieceState
{
    OnBoard,      // 在棋盤上
    InStandby,    // 在待命區（被吃但可重新放置）
    Removed       // 已移除（回到手牌/被消滅）
}

public enum CaptureMode
{
    ToStandby,   // 放到俘虜區
    Remove,      // 直接移除
    ToOpponentHand // 轉換陣營（將棋模式）
}