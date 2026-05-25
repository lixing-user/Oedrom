using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class ManhattanChaserAIService : IAIService
{
    [Inject] private readonly IBoardStateSnapshot _snapshot;
    [Inject] private readonly IBoardQueryService _queryService;
    [Inject] private readonly IMoveValidateService _validator;

    public Task<AIMovement> ExecuteMoveAsync()
    {
        try
        {
            // 1. 取得雙方陣營的棋子 (使用 QueryService 已有的方法)
            var whitePieces = _queryService.GetPiecesForTeam(Team.White);
            var blackPieces = _queryService.GetPiecesForTeam(Team.Black);

            // 若場上無白方目標，或黑方無棋子，則無路可走
            if (whitePieces.Count == 0 || blackPieces.Count == 0)
            {
                return Task.FromResult(AIMovement.NoLegalMoves());
            }

            // 用於儲存所有可能的移動與對應的曼哈頓距離分數
            List<(PieceID pieceId, PiecePosition from, PiecePosition to, int dist)> bestMoves = new();

            // 2. 遍歷所有黑方(AI)棋子
            var dimensions = _queryService.Dimensions;
            foreach (var blackPiece in blackPieces)
            {
                // 取得目前座標 
                var fromPos = blackPiece.CurrentPosition;

                // 窮舉棋盤上的所有座標，並透過 Validator 篩選出合法步
                for (int x = 0; x < dimensions.BoardSizeX; x++)
                {
                    for (int y = 0; y < dimensions.BoardSizeY; y++)
                    {
                        var targetPos = new PiecePosition(x, y);

                        // 驗證是否為該棋子的合法移動
                        if (_validator.IsValidMove(blackPiece.Id, targetPos))
                        {
                            // 計算此落點與「最近的白方棋子」之間的曼哈頓距離
                            int minDist = whitePieces.Min(w =>
                            {
                                var wPos = w.CurrentPosition;
                                return Math.Abs(targetPos.X - wPos.X) + Math.Abs(targetPos.Y - wPos.Y);
                            });

                            bestMoves.Add((blackPiece.Id, fromPos, targetPos, minDist));
                        }
                    }
                }
            }

            // 3. 決策：選擇距離最短的移動動作
            if (bestMoves.Any())
            {
                // 依照距離由小到大排序，取最短距離的移動
                var best = bestMoves.OrderBy(m => m.dist).First();
                Debug.Log($"[ManhattanChaser] 決定移動: 棋子 {best.pieceId} 從 {best.from.X},{best.from.Y} 到 {best.to.X},{best.to.Y}，目標距離: {best.dist}");
                return Task.FromResult(AIMovement.Success(best.pieceId, best.from, best.to));
            }

            return Task.FromResult(AIMovement.NoLegalMoves());
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ManhattanChaser] 執行異常: {ex.Message}");
            return Task.FromResult(AIMovement.EngineError(ex));
        }
    }
}
