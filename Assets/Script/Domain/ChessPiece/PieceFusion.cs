using System.Collections.Generic;
using UnityEngine;

// Domain / Domain Service
public class PieceFusion
{
    private readonly IMovementStrategyFactory _strategyFactory;

    private readonly Dictionary<PieceID, IMovementStrategy> _originalStrategies = new();
    private readonly Dictionary<PieceID, List<IMovementStrategy>> _fusionStrategies = new();

    public PieceFusion(IMovementStrategyFactory strategyFactory)
    {
        _strategyFactory = strategyFactory;
    }

    public void ApplyFusion(ChessPiece piece, ChessPieceType fusionType)
    {
        if (!_originalStrategies.ContainsKey(piece.Id))
        {
            _originalStrategies[piece.Id] = piece.MovementStrategy;
        }

        // 記錄融合策略
        if (!_fusionStrategies.ContainsKey(piece.Id))
        {
            _fusionStrategies[piece.Id] = new List<IMovementStrategy>();
        }

        var fusionStrategy = _strategyFactory.Create(fusionType);
        _fusionStrategies[piece.Id].Add(fusionStrategy);

        UpdatePieceStrategy(piece);
    }

    public void ClearFusions(ChessPiece piece)
    {
        if (_originalStrategies.TryGetValue(piece.Id, out var originalStrategy))
        {
            piece.SetMovementStrategy(originalStrategy);
            _originalStrategies.Remove(piece.Id);
        }

        _fusionStrategies.Remove(piece.Id);
    }

    public bool HasFusions(ChessPiece piece)
    {
        return _fusionStrategies.ContainsKey(piece.Id) &&
               _fusionStrategies[piece.Id].Count > 0;
    }


    private void UpdatePieceStrategy(ChessPiece piece)
    {
        var original = _originalStrategies[piece.Id];
        var fusions = _fusionStrategies[piece.Id];

        if (fusions.Count == 0)
        {
            piece.SetMovementStrategy(original);
            return;
        }

        // 組合所有策略
        var allStrategies = new List<IMovementStrategy> { original };
        allStrategies.AddRange(fusions);

        var compositeStrategy = new CompositeMoveStrategy(allStrategies);
        piece.SetMovementStrategy(compositeStrategy);
    }

    public IReadOnlyList<IMovementStrategy> GetFusionStrategies(ChessPiece piece)
    {
        if (_fusionStrategies.TryGetValue(piece.Id, out var strategies))
            return strategies.AsReadOnly();

        return new List<IMovementStrategy>().AsReadOnly();
    }
}
