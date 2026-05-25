using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public interface IFallbackAIService
{
    Task<AIMovement> CalculateFallbackMoveAsync();
}

#region MainGameField FallbackAI
public class MGFFallbackAI : IFallbackAIService
{
    [Inject] private readonly IBoardQueryService _queryService;
    [Inject] private readonly IMoveValidateService _validator;

    public Task<AIMovement> CalculateFallbackMoveAsync()
    {
        try
        {
            var whitePieces = _queryService.GetPiecesForTeam(Team.White);
            var blackPieces = _queryService.GetPiecesForTeam(Team.Black);

            if (whitePieces.Count == 0 || blackPieces.Count == 0)
            {
                return Task.FromResult(AIMovement.NoLegalMoves());
            }

            // 核心差異：在此處過濾掉 King，確保備用機制絕對不會移動國王
            var availableBlackPieces = blackPieces
                .Where(p => p.Type != ChessPieceType.King)
                .ToList();

            if (availableBlackPieces.Count == 0)
            {
                return Task.FromResult(AIMovement.NoLegalMoves());
            }

            var possibleMoves = new List<(PieceID pieceId, PiecePosition from, PiecePosition to, bool isCapture, int dist)>();
            var dimensions = _queryService.Dimensions;

            // 遍歷所有「過濾後」的黑方(AI)棋子
            foreach (var blackPiece in availableBlackPieces)
            {
                var fromPos = blackPiece.CurrentPosition;

                for (int x = 0; x < dimensions.BoardSizeX; x++)
                {
                    for (int y = 0; y < dimensions.BoardSizeY; y++)
                    {
                        var targetPos = new PiecePosition(x, y);

                        if (_validator.IsValidMove(blackPiece.Id, targetPos))
                        {
                            var targetPiece = _queryService.GetPieceAt(targetPos);
                            bool isCapture = targetPiece != null && targetPiece.Team == Team.White;

                            // 計算此落點與「最近的白方棋子」之間的曼哈頓距離
                            int minDist = whitePieces.Min(w =>
                            {
                                var wPos = w.CurrentPosition;
                                return Math.Abs(targetPos.X - wPos.X) + Math.Abs(targetPos.Y - wPos.Y);
                            });

                            possibleMoves.Add((blackPiece.Id, fromPos, targetPos, isCapture, minDist));
                        }
                    }
                }
            }

            if (possibleMoves.Any())
            {
                // 攻擊性優先：篩選出所有可以吃子的步數
                var captureMoves = possibleMoves.Where(m => m.isCapture).ToList();
                if (captureMoves.Any())
                {
                    // 若有多個吃子選項，取吃完後距離其他敵人最近的位置
                    var bestCapture = captureMoves.OrderBy(m => m.dist).First();
                    Debug.Log($"[MGFFallbackAI] 決定攻擊！棋子 {bestCapture.pieceId} 移動至 {bestCapture.to.X},{bestCapture.to.Y}");
                    return Task.FromResult(AIMovement.Success(bestCapture.pieceId, bestCapture.from, bestCapture.to));
                }

                // 若無法吃子，則執行曼哈頓逼近
                var bestMove = possibleMoves.OrderBy(m => m.dist).First();
                Debug.Log($"[MGFFallbackAI] 決定逼近！棋子 {bestMove.pieceId} 移動至 {bestMove.to.X},{bestMove.to.Y}，目標距離: {bestMove.dist}");
                return Task.FromResult(AIMovement.Success(bestMove.pieceId, bestMove.from, bestMove.to));
            }

            return Task.FromResult(AIMovement.NoLegalMoves());
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MGFFallbackAI] 計算備用步驟時發生錯誤: {ex.Message}");
            return Task.FromResult(AIMovement.NoLegalMoves());
        }
    }
}
#endregion

#region Battle Session & Final Battle FallbackAI
public class FallbackTacticalAI : IFallbackAIService
{
    [Inject] private readonly IBoardQueryService _queryService;
    [Inject] private readonly IMoveValidateService _validator;

    public Task<AIMovement> CalculateFallbackMoveAsync()
    {
        try
        {
            var whitePieces = _queryService.GetPiecesForTeam(Team.White);
            var blackPieces = _queryService.GetPiecesForTeam(Team.Black);

            // 若場上無目標可追，或無棋子可動，回傳無合法步
            if (whitePieces.Count == 0 || blackPieces.Count == 0)
            {
                return Task.FromResult(AIMovement.NoLegalMoves());
            }

            // 儲存格式：(棋子ID, 起點, 終點, 是否為吃子動作, 曼哈頓距離)
            var possibleMoves = new List<(PieceID pieceId, PiecePosition from, PiecePosition to, bool isCapture, int dist)>();
            var dimensions = _queryService.Dimensions;

            // 遍歷所有黑方(AI)棋子
            foreach (var blackPiece in blackPieces)
            {
                var fromPos = blackPiece.CurrentPosition;

                for (int x = 0; x < dimensions.BoardSizeX; x++)
                {
                    for (int y = 0; y < dimensions.BoardSizeY; y++)
                    {
                        var targetPos = new PiecePosition(x, y);

                        if (_validator.IsValidMove(blackPiece.Id, targetPos))
                        {
                            var targetPiece = _queryService.GetPieceAt(targetPos);
                            bool isCapture = targetPiece != null && targetPiece.Team == Team.White;

                            // 計算此落點與「最近的白方棋子」之間的曼哈頓距離
                            int minDist = whitePieces.Min(w =>
                            {
                                var wPos = w.CurrentPosition;
                                return Math.Abs(targetPos.X - wPos.X) + Math.Abs(targetPos.Y - wPos.Y);
                            });

                            possibleMoves.Add((blackPiece.Id, fromPos, targetPos, isCapture, minDist));
                        }
                    }
                }
            }

            if (possibleMoves.Any())
            {
                // 攻擊性優先：篩選出所有可以吃子的步數
                var captureMoves = possibleMoves.Where(m => m.isCapture).ToList();
                if (captureMoves.Any())
                {
                    // 若有多個吃子選項，取吃完後距離其他敵人最近的位置
                    var bestCapture = captureMoves.OrderBy(m => m.dist).First();
                    Debug.Log($"[FallbackAI] 決定攻擊！棋子 {bestCapture.pieceId} 移動至 {bestCapture.to.X},{bestCapture.to.Y}");
                    return Task.FromResult(AIMovement.Success(bestCapture.pieceId, bestCapture.from, bestCapture.to));
                }

                // 若無法吃子，則執行曼哈頓逼近
                var bestMove = possibleMoves.OrderBy(m => m.dist).First();
                Debug.Log($"[FallbackAI] 決定逼近！棋子 {bestMove.pieceId} 移動至 {bestMove.to.X},{bestMove.to.Y}，目標距離: {bestMove.dist}");
                return Task.FromResult(AIMovement.Success(bestMove.pieceId, bestMove.from, bestMove.to));
            }

            return Task.FromResult(AIMovement.NoLegalMoves());
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FallbackAI] 運算發生例外: {ex.Message}");
            return Task.FromResult(AIMovement.EngineError(ex));
        }
    }
}
#endregion