using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

#region 棋子選擇擴展接口
/// <summary>
/// 允許不同場景擴展棋子來源（棋盤、待命區、手牌等）
/// </summary>
public interface IPieceSelectionExtension
{
    string ExtensionName { get; }
    bool IsEnabled { get; set; }
    bool CanSelectPiece(PieceID pieceId, out PieceSelectionContext context);
    List<Vector2Int> GetValidPlacementTiles(PieceID pieceId, PieceSelectionContext context);
    bool TryPlacePiece(PieceID pieceId, PiecePosition targetPos, PieceSelectionContext context);
}

/// <summary>
/// 棋子選擇上下文
/// </summary>
public struct PieceSelectionContext
{
    public PieceSelectionSource Source;  // 來源：棋盤、待命區、手牌等
    public PiecePosition? OriginalPosition;  // 原始位置（如果來自棋盤）
    public object ExtensionData;  // 擴展數據
}

public enum PieceSelectionSource
{
    Board,     // 來自棋盤
    Standby,   // 來自待命區
}
#endregion

#region 擴展點實作
/// <summary>
/// 標準棋盤棋子選擇擴展（默認實現）
/// </summary>
public class BoardPieceSelectionExtension : IPieceSelectionExtension
{
    [InjectOptional] private readonly ITutorialOverrideService _overrideService;

    [Inject] private readonly IGameBoard _board;
    [Inject] private readonly BoardDimensions _dimensions;
    [Inject] private readonly IMoveValidateService _moveValidator;

    public string ExtensionName => "BoardPieceSelection";
    public bool IsEnabled { get; set; } = true;

    public bool CanSelectPiece(PieceID pieceId, out PieceSelectionContext context)
    {
        context = default;

        if (!IsEnabled)
            return false;

        if (_overrideService != null && _overrideService.IsPieceSelectionRestricted(pieceId))
        {
            Debug.LogWarning($"[教學攔截] 目前步驟禁止選取此棋子: {pieceId}");
            // 這裡可以發射 Signal 播放錯誤音效或震動提示
            return false;
        }

        var piece = _board.GetPieceById(pieceId);
        if (piece == null)
            return false;

        // 只處理在棋盤上的棋子
        if (!piece.CurrentPosition.IsValid(_dimensions))
            return false;

        context = new PieceSelectionContext
        {
            Source = PieceSelectionSource.Board,
            OriginalPosition = piece.CurrentPosition
        };

        return true;
    }

    public List<Vector2Int> GetValidPlacementTiles(PieceID pieceId, PieceSelectionContext context)
    {
        var piece = _board.GetPieceById(pieceId);
        if (piece == null)
            return new List<Vector2Int>();

        var validMoves = _moveValidator.GetValidMoves(piece);
        return validMoves.Select(p => new Vector2Int(p.X, p.Y)).ToList();
    }

    public bool TryPlacePiece(PieceID pieceId, PiecePosition targetPos, PieceSelectionContext context)
    {
        // 實際移動由 MoveExecutionStrategy 處理
        // 將驗證工作轉交給專門計算棋盤規則的 Validator
        return _moveValidator.IsValidMove(pieceId, targetPos);
    }
}


/// <summary>
/// 待命區棋子選擇擴展（僅在有待命區的場景綁定）
/// </summary>
public class StandbyPieceSelectionExtension : IPieceSelectionExtension
{
    [Inject] private readonly IStandbyZone _standbyZone; 
    [Inject] private readonly IGameBoard _board;
    [Inject] private readonly BoardDimensions _dimensions;

    public string ExtensionName => "StandbyPieceSelection";
    public bool IsEnabled { get; set; } = true;

    public bool CanSelectPiece(PieceID pieceId, out PieceSelectionContext context)
    {
        context = default;

        if (!IsEnabled)
            return false;

        // 只處理待命區的棋子
        if (!_standbyZone.IsPieceInStandby(pieceId))
            return false;

        bool inStandby = _standbyZone.IsPieceInStandby(pieceId);
        Debug.Log($"[InputDebug] 測試棋子 {pieceId} 是否在 StandbyZone: {inStandby}");

        context = new PieceSelectionContext
        {
            Source = PieceSelectionSource.Standby,
            OriginalPosition = null  // 待命區沒有棋盤位置
        };

        return true;
    }

    public List<Vector2Int> GetValidPlacementTiles(PieceID pieceId, PieceSelectionContext context)
    {
        // 待命區棋子只能放到空格
        var validTiles = new List<Vector2Int>();

        var cornerPositions = new[]
        {
            new Vector2Int(0, 0),                                          // 左下角
            new Vector2Int(0, _dimensions.BoardSizeY - 1),                 // 左上角
            new Vector2Int(_dimensions.BoardSizeX - 1, 0),                 // 右下角
            new Vector2Int(_dimensions.BoardSizeX - 1, _dimensions.BoardSizeY - 1)  // 右上角
        };

        foreach (var corner in cornerPositions)
        {
            var pos = new PiecePosition(corner.x, corner.y);
            if (_board.GetPieceAt(pos) == null)
            {
                validTiles.Add(new Vector2Int(corner.x, corner.y));
            }
        }


        return validTiles;
    }

    public bool TryPlacePiece(PieceID pieceId, PiecePosition targetPos, PieceSelectionContext context)
    {
        // 驗證目標位置為空
        if (_board.GetPieceAt(targetPos) != null)
        {
            Debug.LogWarning($"[StandbyExtension] 目標位置 {targetPos} 已有棋子");
            return false;
        }

        // 2. 驗證目標位置必須在合法的四個角落內
        var validTiles = GetValidPlacementTiles(pieceId, context);
        bool isTargetValid = validTiles.Any(pos => pos.x == targetPos.X && pos.y == targetPos.Y);

        if (!isTargetValid)
        {
            Debug.LogWarning($"[StandbyExtension] 目標位置 {targetPos} 不在合法角落");
            return false;
        }

        //return _board.RestorePiece(pieceId, targetPos);
        return true;
    }
}
#endregion