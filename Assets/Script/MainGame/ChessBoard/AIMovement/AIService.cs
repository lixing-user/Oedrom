using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class AIService : IAIService
{
    [InjectOptional] private readonly ITutorialOverrideService _overrideService;
    [InjectOptional] private readonly IFallbackAIService _fallbackAI;

    [Inject] private readonly IBoardQueryService _queryService;
    [Inject] private readonly IBoardStateSnapshot _snapshot;

    [Inject] private readonly SignalBus _signalBus;
    [Inject] private readonly FENAdapter _fenAdapter;
    [Inject] private readonly FairyStockfishManager _engine;


    public async Task<AIMovement> ExecuteMoveAsync()
    {
        try
        {
            // 1. 【攔截點】：檢查是否有教學強制指令
            if (_overrideService != null && _overrideService.TryGetForcedAIMove(out string forcedMove))
            {
                Debug.Log($"[AIService] 執行教學強制腳本步: {forcedMove}");
                return await SubmitMoveToGameLoop(forcedMove);
            }

            // 2. 正常 AI 邏輯
            if (!_engine.IsEngineReady)
            {
                Debug.Log("[AIService] 引擎尚未啟動，正在初始化引擎...");
                await _engine.InitializeAsync();

                if (!_engine.IsEngineReady)
                {
                    return AIMovement.EngineError(new System.Exception("引擎啟動失敗！"));
                }
            }

            // 設定 AI 策略與難度
            // 你可以根據目前回合、地圖類型、甚至是玩家選擇的難度來動態給值
            int currentSkillLevel = 20;     // 最高難度 20
            int currentAggressiveness = 200; // 高攻擊性 200
            _engine.SetEngineParameters(currentSkillLevel, currentAggressiveness);


            // 2. 取得 FEN 狀態
            string fen = _fenAdapter.GetFEN(_snapshot);
            Debug.Log($"[AIService] 傳送給引擎的 FEN: {fen}");
            _engine.SetPosition(fen);

            // 3. 非同步等待 AI 思考
            Debug.Log("[AIService] AI 開始思考...");
            string bestMove = await _engine.GetBestMoveAsync(1000);

            // 4. 驗證 UCI 輸出
            if (string.IsNullOrEmpty(bestMove))
            {
                Debug.LogWarning("[AIService] AI 返回空指令，嘗試使用後備 AI...");
                if (_fallbackAI != null)
                {
                    return await _fallbackAI.CalculateFallbackMoveAsync();
                }
                return AIMovement.EngineError(new System.Exception("AI 返回空指令，且無後備 AI 可用"));
            }
            // === 明確攔截終局狀態 ===
            if (bestMove.Contains("(none)"))
            {
                if (_fallbackAI != null)
                    return await _fallbackAI.CalculateFallbackMoveAsync();

                Debug.Log("[AIService] 引擎回傳 (none)，判定為無合法步（Checkmate 或 Stalemate）。");

                // === 改良：直接調用已建立好的領域物件工廠方法 ===
                return AIMovement.NoLegalMoves();
            }

            Debug.Log($"[AIService] AI 決定走法: {bestMove}");

            // 5. 轉換為領域指令並提交
            return await SubmitMoveToGameLoop(bestMove);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AIService] AI 執行異常: {ex.Message}");
            return AIMovement.EngineError(ex);
        }
    }

    private async Task<AIMovement> SubmitMoveToGameLoop(string uciMove)
    {
        // 步驟 1：使用 UCIParser 解析協議
        var parseResult = UCIParser.Parse(uciMove);
        if (!parseResult.IsSuccess)
        {
            Debug.LogWarning($"UCI 解析失敗: {parseResult.Error}");
            return AIMovement.ParseError(uciMove, parseResult.Error);
        }

        var uciMoveData = parseResult.Value;
        var from = new PiecePosition(uciMoveData.FromX, uciMoveData.FromY);
        var to = new PiecePosition(uciMoveData.ToX, uciMoveData.ToY);

        // 獲取要移動的棋子 ID
        var piece = _queryService.GetPieceAt(from);
        if (piece == null) return AIMovement.DomainValidationFailed("起始位置找不到棋子");

        // 透過判斷注入的 fallbackservice 是否為專屬的 MGFFallbackAI，來確認當前是否為 chessboardt
        if (piece.Type == ChessPieceType.King && _fallbackAI is MGFFallbackAI)
        {
            Debug.Log("[AIService] Stockfish 最佳移動為 King，觸發 MGF 場景回滾機制");
            return await _fallbackAI.CalculateFallbackMoveAsync();
        }

        return AIMovement.Success(piece.Id, from, to);
    }
}

public interface IAIService
{
    Task<AIMovement> ExecuteMoveAsync();
}