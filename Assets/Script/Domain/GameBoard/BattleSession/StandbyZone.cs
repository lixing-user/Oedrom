using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 待命區接口（Domain Service）
/// </summary>
public interface IStandbyZone
{
    void AddPiece(ChessPiece piece);
    bool RemovePiece(PieceID pieceId);
    bool TryRevivePiece(PieceID pieceId, PiecePosition position);
    IEnumerable<ChessPiece> GetStandbyPieces(Team team);    // 獲取指定隊伍的待命區棋子
    ChessPiece GetPieceById(PieceID pieceId);
    bool IsPieceInStandby(PieceID pieceId);     // 檢查棋子是否在待命區
    void Clear(Team? team = null);      // 清空待命區（遊戲結束時）
    int GetPieceCount(Team team);
}


public class StandbyZone : IStandbyZone
{
    private readonly Dictionary<Team, List<ChessPiece>> _standbyPieces;
    private readonly Dictionary<PieceID, ChessPiece> _pieceById;

    public StandbyZone()
    {
        _standbyPieces = new Dictionary<Team, List<ChessPiece>>
        {
            { Team.White, new List<ChessPiece>() },
            { Team.Black, new List<ChessPiece>() }
        };

        _pieceById = new Dictionary<PieceID, ChessPiece>();
    }


    public void AddPiece(ChessPiece piece)
    {
        Debug.Log("2. StandbyZone 真正收到了 AddPiece 呼叫");

        if (piece == null)
        {
            Debug.LogWarning("[StandbyZone] 嘗試加入空棋子");
            return;
        }

        // 驗證狀態
        if (piece.State != PieceState.InStandby)
        {
            Debug.LogWarning($"[StandbyZone] 棋子 {piece.Id} 狀態不是 InStandby，當前: {piece.State}");
            return;
        }

        if (_pieceById.ContainsKey(piece.Id))
        {
            Debug.LogWarning($"[StandbyZone] 棋子 {piece.Id} 已在待命區");
            return;
        }

        // 加入數據結構
        _standbyPieces[piece.Team].Add(piece);
        _pieceById[piece.Id] = piece;

        Debug.Log($"[StandbyZone] {piece.Team} 方 {piece.Type}({piece.Id}) 已加入待命區");
    }

    public bool RemovePiece(PieceID pieceId)
    {
        if (!_pieceById.TryGetValue(pieceId, out var piece))
        {
            Debug.LogWarning($"[StandbyZone] 找不到棋子: {pieceId}");
            return false;
        }

        _standbyPieces[piece.Team].Remove(piece);
        _pieceById.Remove(pieceId);

        Debug.Log($"[StandbyZone] {piece.Type}({pieceId}) 已從待命區移除");
        return true;
    }


    /// <summary>
    /// 恢復棋子到棋盤（需配合 GameBoard.RestorePiece）
    /// </summary>
    public bool TryRevivePiece(PieceID pieceId, PiecePosition position)
    {
        if (!_pieceById.TryGetValue(pieceId, out var piece))
        {
            Debug.LogWarning($"[StandbyZone] 棋子 {pieceId} 不在待命區");
            return false;
        }

        Debug.Log($"[StandbyZone] 嘗試復活 {piece.Type}({pieceId}) 到 {position}");

        // 只負責標記，實際放置由 GameBoard 處理
        // 這裡不移除，由 GameBoard.RestorePiece 成功後再移除
        return true;
    }


    public IEnumerable<ChessPiece> GetStandbyPieces(Team team)
    {
        return _standbyPieces.TryGetValue(team, out var pieces)
            ? pieces.ToList()  // 返回副本，防止外部修改
            : Enumerable.Empty<ChessPiece>();
    }

    public ChessPiece GetPieceById(PieceID pieceId)
    {
        if (pieceId == null) return null;
        return _pieceById.TryGetValue(pieceId, out var piece) ? piece : null;
    }

    public bool IsPieceInStandby(PieceID pieceId)
    {
        return _pieceById.ContainsKey(pieceId);
    }

    public int GetPieceCount(Team team)
    {
        return _standbyPieces.TryGetValue(team, out var pieces)
            ? pieces.Count
            : 0;
    }


    /// <summary>
    /// 清空待命區
    /// </summary>
    public void Clear(Team? team = null)
    {
        if (team.HasValue)
        {
            // 清空指定隊伍
            var pieces = _standbyPieces[team.Value].ToList();
            foreach (var piece in pieces)
            {
                _pieceById.Remove(piece.Id);
            }
            _standbyPieces[team.Value].Clear();

            Debug.Log($"[StandbyZone] {team.Value} 方待命區已清空");
        }
        else
        {
            // 清空所有
            _pieceById.Clear();
            foreach (var list in _standbyPieces.Values)
            {
                list.Clear();
            }

            Debug.Log("[StandbyZone] 所有待命區已清空");
        }
    }
}
