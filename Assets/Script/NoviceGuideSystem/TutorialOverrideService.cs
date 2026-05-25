using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface ITutorialOverrideService
{
    // --- 系統功能鎖定 (Feature Lock) ---
    void LockFeature(string featureName);
    void UnlockFeature(string featureName);
    bool IsFeatureLocked(string featureName);

    // --- AI 覆寫 ---
    void EnqueueForcedAIMove(string uciMove);
    bool TryGetForcedAIMove(out string uciMove);

    // --- 隨機事件覆寫 ---
    void EnqueueForcedEvent(RandomEventData eventData);
    bool TryGetForcedEvent(out RandomEventData eventData);

    // --- 手牌/牌組覆寫 (未來擴充) ---
    void SetRestrictedCardPlay(ChessPieceType allowedCardType);
    bool IsCardPlayRestricted(ChessPieceType targetCardType);

    // --- 空間位置覆寫 (限制落點) ---
    void SetRestrictedPlacement(Vector2Int allowedPos);
    bool IsPlacementRestricted(Vector2Int targetPos);

    void SetRestrictedMove(Vector2Int allowedPos);
    bool IsMoveRestricted(Vector2Int targetPos);
    void LockAllPieceSelection();
    void SetAllowedPieceSelection(PieceID allowedId);
    bool IsPieceSelectionRestricted(PieceID targetPieceId);

    // --- 安全機制 ---
    void ClearAllOverrides();
    void ClearRestrictions();
}

public class TutorialOverrideService : ITutorialOverrideService
{
    [Inject] private readonly SignalBus _signalBus;

    private HashSet<string> _lockedFeatures = new HashSet<string>();
    private Queue<string> _forcedAIMoves = new Queue<string>();
    private Queue<RandomEventData> _forcedEvents = new Queue<RandomEventData>();
    //private Queue<string> _forcedCardDraws = new Queue<string>();

    private bool _isAllPieceSelectionLocked = false;
    private Vector2Int? _allowedPlacementPos;
    private Vector2Int? _allowedMovePos;

    private ChessPieceType? _allowedCardType;
    private PieceID _allowedPieceId = null;

    #region Feature Lock Overrides
    public void LockFeature(string featureName)
    {
        _lockedFeatures.Add(featureName);
        Debug.Log($"[TutorialOverride] 全域功能已封印: {featureName}");
        _signalBus.Fire(new FeatureLockChangedSignal(featureName, true));
    }

    public void UnlockFeature(string featureName)
    {
        _lockedFeatures.Remove(featureName);
        Debug.Log($"[TutorialOverride] 全域功能已解除封印: {featureName}");
        _signalBus.Fire(new FeatureLockChangedSignal(featureName, false));
    }

    public bool IsFeatureLocked(string featureName)
    {
        return _lockedFeatures.Contains(featureName);
    }
    #endregion

    #region AI Move Overrides
    public void EnqueueForcedAIMove(string uciMove)
    {
        Debug.Log($"[TutorialOverride] 安插強制 AI 移動: {uciMove}");
        _forcedAIMoves.Enqueue(uciMove);
    }

    public bool TryGetForcedAIMove(out string uciMove)
    {
        if (_forcedAIMoves.Count > 0)
        {
            uciMove = _forcedAIMoves.Dequeue();
            return true;
        }
        uciMove = null;
        return false;
    }
    #endregion

    #region Random Event Overrides
    public void EnqueueForcedEvent(RandomEventData eventData) => _forcedEvents.Enqueue(eventData);
    public bool TryGetForcedEvent(out RandomEventData eventData)
    {
        if (_forcedEvents.Count > 0)
        {
            eventData = _forcedEvents.Dequeue();
            return true;
        }
        eventData = null;
        return false;
    }
    #endregion

    #region ChessBoard Placement & Movement Overrides
    public void SetRestrictedPlacement(Vector2Int allowedPos)
    {
        _allowedPlacementPos = allowedPos;
    }

    public bool IsPlacementRestricted(Vector2Int targetPos)
    {
        // 如果沒有設定限制，就代表不限制 (回傳 false 代表 "沒有被管制")
        if (_allowedPlacementPos == null) return false;

        // 如果位置不對，或拿錯兵種，就視為被限制（被攔截）
        if (targetPos != _allowedPlacementPos.Value)
        {
            return true; // 攔截！
        }
        return false; // 通過！
    }

    public void SetRestrictedMove(Vector2Int allowedPos) => _allowedMovePos = allowedPos;

    public bool IsMoveRestricted(Vector2Int targetPos)
    {
        if (_allowedMovePos == null) return false;
        return targetPos != _allowedMovePos.Value;
    }

    public void LockAllPieceSelection()
    {
        _isAllPieceSelectionLocked = true;
    }

    public void SetAllowedPieceSelection(PieceID allowedId)
    {
        _allowedPieceId = allowedId;
    }

    public bool IsPieceSelectionRestricted(PieceID targetPieceId)
    {
        // 1. 如果全域上鎖，任何棋子都被拒絕
        if (_isAllPieceSelectionLocked) return true;

        // 2. 如果有指定唯一合法的棋子，且點擊的不是它，則拒絕
        if (_allowedPieceId != null && targetPieceId != _allowedPieceId) return true;

        // 否則放行
        return false;
    }
    #endregion

    #region Card Play Overrides
    public void SetRestrictedCardPlay(ChessPieceType allowedCardType)
    {
        _allowedCardType = allowedCardType;
    }

    public bool IsCardPlayRestricted(ChessPieceType targetCardType)
    {
        if (_allowedCardType == null) return false;
        return targetCardType != _allowedCardType.Value;
    }
    #endregion

    public void ClearAllOverrides()
    {
        Debug.LogWarning("[TutorialOverride] 清除所有強制覆寫狀態！(可能是教學結束或中斷)");
        _lockedFeatures.Clear();
        _forcedAIMoves.Clear();
        _forcedEvents.Clear();
        ClearRestrictions();    
        //_forcedCardDraws.Clear();
    }

    public void ClearRestrictions()
    {
        _allowedPlacementPos = null;
        _allowedMovePos = null;
        _allowedCardType = null;
        _isAllPieceSelectionLocked = false;
        _allowedPieceId = null;
    }
}
