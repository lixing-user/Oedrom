using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

// Domain/Aggregates/GameBoard.cs
public class GameBoard<TTile> : IGameBoard where TTile : Tile
{
    private readonly BoardContextId _contextId;
    private readonly BoardDimensions _dimensions;
    private readonly ICaptureHandler _captureHandler;
    [InjectOptional] private readonly IStandbyZone _standbyZone;

    private readonly Dictionary<PiecePosition, TTile> _tiles = new();

    private readonly Dictionary<PiecePosition, ChessPiece> _pieces = new();
    private readonly Dictionary<PieceID, ChessPiece> _pieceById = new();

    public BoardDimensions Dimensions => _dimensions;


    public GameBoard(BoardDimensions dimensions, ICaptureHandler captureHandler, BoardContextId contextId)
    {
        _dimensions = dimensions;
        _captureHandler = captureHandler;
        _contextId = contextId;
    }


    #region 業務邏輯
    
    #region ChessPiece
    public bool MovePiece(PiecePosition from, PiecePosition to)
    {
        if (!_pieces.TryGetValue(from, out var piece))
        {
            // 如果印出這行，代表白兵真的從字典的 (5, 3) 蒸發了！
            UnityEngine.Debug.LogError($"[GameBoard] MovePiece 失敗：字典中找不到位於 {from} 的棋子！");
            return false;
        }

        if (!to.IsValid(_dimensions))
        {
            UnityEngine.Debug.LogError($"[GameBoard] MovePiece 失敗：目標座標 {to} 超出邊界！");
            return false;
        }

        var targetTile = GetTileAt(to);
        if (targetTile != null && !targetTile.CanEnter(piece.Type))
        {
            // 如果印出這行，代表地形判定出了問題！
            UnityEngine.Debug.LogError($"[GameBoard] MovePiece 失敗：地形限制，無法進入 {to}！");
            return false;
        }


        // 業務規則：移動邏輯
        _pieces.Remove(from);
        piece.MoveTo(to);
        _pieces[to] = piece;

        RaiseEvent(new PieceMovedEvent(piece.Id, from, to, piece.Type, piece.Team, _contextId));
        return true;
    }

    public void PlacePiece(ChessPiece piece, PiecePosition position)
    {
        if (!position.IsValid(_dimensions))
            throw new InvalidOperationException("位置超出棋盤");

        if (_pieces.ContainsKey(position))
            throw new InvalidOperationException("位置已有棋子");

        piece.MoveTo(position);

        piece.ChangeState(PieceState.OnBoard);

        _pieces[position] = piece;

        _pieceById[piece.Id] = piece;

        if (piece.SourceCardId != null)
        {
            RaiseEvent(new PieceSpawnedFromCardEvent(
                piece.Id,
                piece.SourceCardId,
                piece.Type,
                position,
                piece.Team,
                _contextId
            ));
        }
    }

    public bool CapturePiece(PiecePosition position, ChessPiece capturer = null)
    {
        if (!_pieces.TryGetValue(position, out var capturedPiece))
            return false;

        var result = _captureHandler.HandleCapture(capturedPiece, capturer, position);

        if (!result.Success) return false;
        

        _pieces.Remove(position);
        if (result.Mode == CaptureMode.Remove)
        {
            _pieceById.Remove(capturedPiece.Id);  // 永久移除
        }

        RaiseEvent(new PieceCapturedEvent(
           capturedPiece.Id,
           capturedPiece.Type,
           position,
           capturedPiece.Team,
           _contextId,
           result.Mode
       ));
        return true;
    }

    public bool RemovePiece(PiecePosition position)
    {
        if (!_pieces.TryGetValue(position, out var piece))
            return false;

        _pieces.Remove(position);
        _pieceById.Remove(piece.Id);
        piece.ChangeState(PieceState.Removed);

        // 復用現有的 PieceCapturedEvent，用 Remove 模式
        RaiseEvent(new PieceCapturedEvent(
            piece.Id,
            piece.Type,
            position,
            piece.Team,
            _contextId,
            CaptureMode.Remove  // 直接移除，不走 CaptureHandler
        ));
        return true;
    }

    public bool RestorePiece(PieceID pieceId, PiecePosition newPosition)
    {
        if (!_pieceById.TryGetValue(pieceId, out var piece))
        {
            // 2. [後門] 如果找不到，有可能是憑空生成的實體，向待命區要人
            if (_standbyZone != null)
            {
                piece = _standbyZone.GetPieceById(pieceId);
            }

            if (piece != null)
            {
                // 3. 補辦登記！將生成的棋子正式納入 GameBoard 的管轄字典中
                _pieceById[pieceId] = piece;
                UnityEngine.Debug.Log($"[GameBoard] 為憑空生成的棋子 {pieceId} 補辦註冊");
            }
            else
            {
                UnityEngine.Debug.LogError($"[GameBoard] RestorePiece 失敗：完全找不到 {pieceId}");
                return false;
            }
        }

        if (!_standbyZone.IsPieceInStandby(pieceId))
            return false;

        if (piece.State != PieceState.InStandby) return false;
        if (!newPosition.IsValid(_dimensions)) return false;
        if (_pieces.ContainsKey(newPosition)) return false;

        _standbyZone.RemovePiece(pieceId);       // Step 5: 從待命區移除
        piece.RestoreFromCapture(newPosition);       // Step 3: 調用 Entity 方法恢復狀態
        _pieces[newPosition] = piece;    // Step 4: 放回棋盤

        RaiseEvent(new PieceTeamChangedEvent(
            piece.Id,
            piece.Team,  // 舊隊伍
            piece.Team,  // 新隊伍（復原後保持不變）
            piece.Type,
            _contextId
        ));

        RaiseEvent(new PieceRestoredEvent(
            piece.Id,
            newPosition,
            piece.Type,
            piece.Team,
            _contextId
        ));

        return true;
    }

    public bool SwapPieces(PiecePosition pos1, PiecePosition pos2)
    {
        if (!_pieces.TryGetValue(pos1, out var piece1))
            return false;
        if (!_pieces.TryGetValue(pos2, out var piece2))
            return false;

        if (piece1.IsEnemyOf(piece2))
            return false;

        if (!CanPieceEnterTile(piece1, pos2) || !CanPieceEnterTile(piece2, pos1))
            return false;

        return ExecuteSwap(piece1, piece2, pos1, pos2);
    }
    #endregion

    #region Tile
    /// 添加 Tile 到棋盤（初始化階段使用）
    public void AddTile(TTile tile)
    {
        if (!tile.Position.IsValid(_dimensions))
            throw new InvalidOperationException("Tile 位置超出棋盤範圍");

        if (_tiles.ContainsKey(tile.Position))
            throw new InvalidOperationException($"位置 {tile.Position} 已存在 Tile");

        _tiles[tile.Position] = tile;
    }

    /// 批次添加 Tiles
    public void AddTiles(IEnumerable<TTile> tiles)
    {
        foreach (var tile in tiles)
        {
            AddTile(tile);
        }
    }
    #endregion

    public void ClearBoard()
    {
        // 1. 清空座標與實體的對照表
        _pieces.Clear();
        _pieceById.Clear();

        // 2. 清空所有的領域事件，防止觸發不必要的視覺信號
        ClearEvents();
    }

    #endregion


    #region 私有執行方法
    private bool ExecuteSwap(ChessPiece piece1, ChessPiece piece2,
                            PiecePosition pos1, PiecePosition pos2)
    {
        _pieces.Remove(pos1);
        _pieces.Remove(pos2);

        piece1.MoveTo(pos2);
        piece2.MoveTo(pos1);

        _pieces[pos2] = piece1;
        _pieces[pos1] = piece2;

        RaiseEvent(new PiecesSwappedEvent(
            piece1.Id, piece2.Id,
            pos1, pos2,
            piece1.Type, piece2.Type,
            piece1.Team,
            _contextId
        ));

        return true;
    }
    #endregion


    #region 驗證輔助方法

    private bool ValidatePositions(PiecePosition from, PiecePosition to, out ChessPiece piece)
    {
        piece = null;

        if (!_pieces.TryGetValue(from, out piece))
            return false;

        if (!to.IsValid(_dimensions))
            return false;

        if (piece.IsCaptured)
            return false;

        if (from.Equals(to))
            return false;

        return true;
    }

    private bool CanPieceEnterTile(ChessPiece piece, PiecePosition position)
    {
        var tile = GetTileAt(position);
        return tile == null || tile.CanEnter(piece.Type);
    }

    #endregion


    #region 查詢方法
    public TTile GetTileAt(PiecePosition position) =>
        _tiles.TryGetValue(position, out var tile) ? tile : null;

    public ChessPiece GetPieceById(PieceID id)
    {
        return _pieceById.TryGetValue(id, out var piece) ? piece : null;
    }

    public ChessPiece GetPieceAt(PiecePosition position)
    {
        return _pieces.TryGetValue(position, out var piece) ? piece : null;
    }

    // 獲取所有棋子（用於保存或查詢）
    public IEnumerable<ChessPiece> GetAllPieces() => _pieceById.Values;  /// 獲取所有棋子（包含棋盤上和俘虜區的）
    public IEnumerable<ChessPiece> GetAllPiecesOnBoard() => _pieces.Values;    /// 獲取棋盤上的所有棋子（不包含俘虜區）

    // 獲取指定隊伍的所有棋子
    public IEnumerable<ChessPiece> GetPiecesByTeam(Team team)
    {
        return _pieces.Values.Where(p => p.Team == team);
    }
    public int GetPieceCountByTeam(Team team)
    {
        return _pieces.Values.Count(p => p.Team == team);
    }

    public int GetBoardHeight() => _dimensions.BoardSizeY;
    public int GetBoardWidth() => _dimensions.BoardSizeX;

    public List<PiecePosition> GetAllEmptyPositions()
    {
        var emptyPositions = new List<PiecePosition>();

        for (int x = 0; x < _dimensions.BoardSizeX; x++)
        {
            for (int y = 0; y < _dimensions.BoardSizeY; y++)
            {
                var pos = new PiecePosition(x, y);

                // 1. 檢查該位置是否沒有棋子
                if (!_pieces.ContainsKey(pos))
                {
                    emptyPositions.Add(pos);
                }
            }
        }
        return emptyPositions;
    }
    #endregion


    #region 領域事件處理
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearEvents() => _domainEvents.Clear();

    private void RaiseEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    #endregion
}