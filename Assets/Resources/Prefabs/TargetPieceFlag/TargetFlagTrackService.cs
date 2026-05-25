using System.Collections.Generic;
using UnityEngine;

public interface ITargetPieceTracker
{
    void RegisterTarget(PieceID pieceId);
    void UnregisterTarget(PieceID pieceId);
    void ClearAllTargets();
    IEnumerable<PieceID> GetActiveTargets();
    bool HasAnyTarget();
}

public class TargetFlagTrackService : ITargetPieceTracker
{
    private readonly HashSet<PieceID> _targetPieceIds = new HashSet<PieceID>();

    public void RegisterTarget(PieceID pieceId)
    {
        _targetPieceIds.Add(pieceId);
    }

    public void UnregisterTarget(PieceID pieceId)
    {
        _targetPieceIds.Remove(pieceId);
    }

    public void ClearAllTargets()
    {
        _targetPieceIds.Clear();
    }

    public IEnumerable<PieceID> GetActiveTargets()
    {
        return _targetPieceIds;
    }

    public bool HasAnyTarget()
    {
        return _targetPieceIds.Count > 0;
    }
}