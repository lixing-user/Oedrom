using System.Linq;
using UnityEngine;
using Zenject;

public class VictoryResult
{
    public bool IsGameOver { get; set; }
    public Team? Winner { get; set; }
    public string Reason { get; set; }
}

public interface IVictoryCheckService
{
    VictoryResult CheckVictoryConditions();
}

public class VictoryCheckService : IVictoryCheckService
{
    [Inject] private readonly IBoardQueryService _boardQuery;
    [Inject] private readonly ITargetPieceTracker _targetPieceTracker;

    public VictoryResult CheckVictoryConditions()
    {
        // 條件 1：黑方的王是否死亡
        var blackPieces = _boardQuery.GetPiecesForTeam(Team.Black);
        bool isBlackKingAlive = blackPieces.Any(p => p.Type == ChessPieceType.King && !p.IsCaptured);

        if (!isBlackKingAlive)
        {
            return new VictoryResult { IsGameOver = true, Winner = Team.White, Reason = "黑方國王被擊敗" };
        }

        // 條件 2：黑方的 Pawn 是否進入了 y=0 的格子
        bool hasBlackPawnReachedEnd = blackPieces.Any(p =>
            p.Type == ChessPieceType.Pawn &&
            !p.IsCaptured &&
            p.CurrentPosition.Y == 0); // 假設 Y=0 是白方底線

        if (hasBlackPawnReachedEnd)
        {
            return new VictoryResult { IsGameOver = true, Winner = Team.Black, Reason = "黑方士兵抵達底線" };
        }

        // 條件 3：白方沒手牌或手牌皆為 broken 狀態
        // 這裡需要根據你的手牌系統實作
        /*
        var whiteHandCards = _handCardService.GetHandCards(Team.White);
        bool noPlayableCards = whiteHandCards.Count == 0 || whiteHandCards.All(c => c.IsBroken);
        
        // 注意：除了手牌，可能還要檢查場上是否還有白方棋子可以移動，以免誤判
        var whitePiecesOnBoard = _boardQuery.GetPiecesForTeam(Team.White).Where(p => !p.IsCaptured);

        if (noPlayableCards && !whitePiecesOnBoard.Any())
        {
            return new VictoryResult { IsGameOver = true, Winner = Team.Black, Reason = "白方資源耗盡" };
        }
        */

        // 條件 4：白方護衛目標 (TargetFlag) 是否陣亡
        if (_targetPieceTracker != null && _targetPieceTracker.HasAnyTarget())
        {
            var targetPiece = _targetPieceTracker.GetActiveTargets();
            foreach (var pieceId in targetPiece)
            {
                var piece = _boardQuery.GetPieceById(pieceId);

                // 檢查該 ID 的棋子是否不存在或已被標記為捕獲
                if (piece == null || piece.IsCaptured)
                {
                    return new VictoryResult
                    {
                        IsGameOver = true,
                        Winner = Team.Black,
                        Reason = "護衛目標陣亡" // 可配合 EndReason Enum 使用
                    };
                }
            }
        }
            

        // 尚無人獲勝
        return new VictoryResult { IsGameOver = false, Winner = null, Reason = "" };
    }
}
