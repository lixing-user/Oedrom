//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using UnityEngine;
//using Zenject;

///// <summary>
///// 給 AIService 用的「哨兵檢查」介面。
///// 不論 Stockfish 這回合是否成功回應,都應該先問一次:
///// 現在是不是有「立即決定勝負等級」的手(能直接吃掉對方 target piece,
///// 或我方 target piece 下一手就會被吃)。這種局面下,Stockfish 的建議
///// (即使合法、即使看起來正常)完全不可信,因為它根本不知道這顆棋子
///// 是勝負關鍵。
///// </summary>
//public interface ITacticalAdvisorService
//{
//    /// <summary>
//    /// 回傳 Tier 0 / Tier 1 等級的關鍵手;若目前沒有這種局面,回傳 null,
//    /// 代表可以放心採用 Stockfish 的建議。
//    /// </summary>
//    AIMovement TryGetCriticalMove(Team aiTeam, Team opponentTeam);
//}

//public class PriorityTacticalAI : IAIService, IFallbackAIService, ITacticalAdvisorService
//{
//    [Inject] private readonly IBoardQueryService _queryService;
//    [Inject] private readonly IMoveValidateService _validator;
//    [Inject] private readonly ITargetPieceService _targetPieceService;

//    private const Team AITeam = Team.Black;
//    private const Team OpponentTeam = Team.White;

//    private readonly bool _excludeKingFromMoves;

//    // 持久化:目前指派去獵殺目標的棋子。不用每回合重新挑選,
//    // 才不會出現「這手換這顆子逼近,下手又換另一顆子逼近」的原地繞圈。
//    private PieceID? _currentHunterId;

//    public PriorityTacticalAI(bool excludeKingFromMoves = false)
//    {
//        _excludeKingFromMoves = excludeKingFromMoves;
//    }

//    // ================= IAIService =================
//    public Task<AIMovement> ExecuteMoveAsync() => CalculateFallbackMoveAsync();

//    // ================= IFallbackAIService =================
//    public Task<AIMovement> CalculateFallbackMoveAsync()
//    {
//        try
//        {
//            var critical = TryGetCriticalMove(AITeam, OpponentTeam);
//            if (critical != null) return Task.FromResult(critical);

//            var hunt = TryHuntEnemyTarget();
//            if (hunt != null) return Task.FromResult(hunt);

//            var greedy = TryGreedyCaptureOrApproach();
//            if (greedy != null) return Task.FromResult(greedy);

//            return Task.FromResult(AIMovement.NoLegalMoves());
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"[PriorityTacticalAI] 計算後備步驟時發生例外: {ex.Message}");
//            return Task.FromResult(AIMovement.EngineError(ex));
//        }
//    }

//    // ================= ITacticalAdvisorService =================
//    public AIMovement TryGetCriticalMove(Team aiTeam, Team opponentTeam)
//    {
//        // Tier 0: 能直接吃掉對方 target piece → 遊戲結束,最高優先,不用再問 Stockfish。
//        var enemyTarget = _targetPieceService.GetTargetPiece(opponentTeam);
//        if (enemyTarget != null)
//        {
//            var winningMove = FindCaptureMove(aiTeam, enemyTarget.CurrentPosition);
//            if (winningMove != null)
//            {
//                Debug.Log("[PriorityTacticalAI] Tier0 命中:可直接吃掉對方 target piece。");
//                return winningMove;
//            }
//        }

//        // Tier 1: 我方 target piece 下一手就會被吃 → 必須優先處理。
//        var myTarget = _targetPieceService.GetTargetPiece(aiTeam);
//        if (myTarget != null &&
//            _queryService.CanAnyPieceReach(_validator, opponentTeam, myTarget.CurrentPosition))
//        {
//            var defense = FindDefensiveMove(aiTeam, opponentTeam, myTarget);
//            if (defense != null)
//            {
//                Debug.Log("[PriorityTacticalAI] Tier1 命中:我方 target piece 告急,採用防禦手。");
//                return defense;
//            }
//        }

//        return null;
//    }

//    #region 內部邏輯
//    private AIMovement FindCaptureMove(Team aiTeam, PiecePosition targetPos)
//    {
//        foreach (var piece in GetMovablePool(aiTeam))
//        {
//            if (_validator.IsValidMove(piece.Id, targetPos))
//                return AIMovement.Success(piece.Id, piece.CurrentPosition, targetPos);
//        }
//        return null;
//    }

//    private AIMovement FindDefensiveMove(Team aiTeam, Team opponentTeam, ChessPiece myTarget)
//    {
//        // 選項 A:target piece 自己找一個「白方目前任何活著的棋子都走不到」的合法格。
//        var targetMoves = GetLegalMoves(myTarget);
//        var safeMove = targetMoves
//            .Where(m => !_queryService.CanAnyPieceReach(_validator, opponentTeam, m.to))
//            .OrderByDescending(m => m.isCapture) // 逃跑順便能吃子更好
//            .FirstOrDefault();

//        if (safeMove.to != null)
//        {
//            Debug.Log($"[PriorityTacticalAI] Target piece 告急,撤往安全格 ({safeMove.to.X},{safeMove.to.Y})。");
//            return AIMovement.Success(myTarget.Id, myTarget.CurrentPosition, safeMove.to);
//        }

//        // 選項 B:找不到安全格,嘗試直接吃掉威脅來源。
//        // (無法處理「擋住滑動棋子攻擊路線」這種防守方式,見檔頭已知限制。)
//        var attackers = _queryService.GetPiecesThatCanReach(_validator, opponentTeam, myTarget.CurrentPosition);
//        foreach (var attacker in attackers)
//        {
//            var counter = FindCaptureMove(aiTeam, attacker.CurrentPosition);
//            if (counter != null)
//            {
//                Debug.Log($"[PriorityTacticalAI] 無安全格可退,反吃威脅者 {attacker.Id}。");
//                return counter;
//            }
//        }

//        Debug.LogWarning("[PriorityTacticalAI] Target piece 面臨無法化解的威脅(無安全格、無法反吃)。");
//        return null;
//    }

//    private AIMovement TryHuntEnemyTarget()
//    {
//        var enemyTarget = _targetPieceService.GetTargetPiece(OpponentTeam);
//        if (enemyTarget == null) return null; // 尚未設定 target piece,交給下一層的舊版貪婪解

//        var myPieces = GetMovablePool(AITeam);
//        if (myPieces.Count == 0) return null;

//        // 檢查目前指派的獵手是否仍然存在且活著(GetMovablePool 已濾掉 IsCaptured,
//        // 所以獵手被吃掉時這裡自然找不到,會正確觸發重新指派)。
//        ChessPiece hunter = null;
//        if (_currentHunterId.HasValue)
//        {
//            hunter = myPieces.FirstOrDefault(p => p.Id == _currentHunterId.Value);
//            if (hunter == null) _currentHunterId = null;
//        }

//        // 尚未指派,或剛被清空 → 挑一顆「離目標子最近、且真的有合法動作」的子當獵手
//        if (hunter == null)
//        {
//            hunter = myPieces
//                .Where(p => GetLegalMoves(p).Count > 0)
//                .OrderBy(p => ManhattanDistance(p.CurrentPosition, enemyTarget.CurrentPosition))
//                .FirstOrDefault();

//            if (hunter == null) return null; // 全隊都動不了,交給上層判斷

//            _currentHunterId = hunter.Id;
//            Debug.Log($"[PriorityTacticalAI] 指派新獵手 {hunter.Id},追擊目標子 " +
//                      $"({enemyTarget.CurrentPosition.X},{enemyTarget.CurrentPosition.Y})。");
//        }

//        var legalMoves = GetLegalMoves(hunter);
//        if (legalMoves.Count == 0)
//        {
//            // 獵手被困住(可能被包圍),強制換手,下次呼叫時重新指派。
//            Debug.Log($"[PriorityTacticalAI] 獵手 {hunter.Id} 已無合法動作,釋出獵手身份。");
//            _currentHunterId = null;
//            return null;
//        }

//        // 獵手的合法手中:優先選「能吃到子」的,其次選「離目標子最近」的。
//        var best = legalMoves
//            .OrderByDescending(m => m.isCapture)
//            .ThenBy(m => ManhattanDistance(m.to, enemyTarget.CurrentPosition))
//            .First();

//        return AIMovement.Success(hunter.Id, hunter.CurrentPosition, best.to);
//    }

//    private AIMovement TryGreedyCaptureOrApproach()
//    {
//        // 安全網:等同舊版 FallbackTacticalAI 的邏輯,確保在 target piece
//        // 尚未設定,或以上分層都找不到手時,仍然回傳一個合法著法。
//        var whitePieces = GetAlivePieces(OpponentTeam);
//        var blackPieces = GetMovablePool(AITeam);
//        if (whitePieces.Count == 0 || blackPieces.Count == 0) return null;

//        var candidates = new List<(PieceID pieceId, PiecePosition from, PiecePosition to, bool isCapture, int dist)>();

//        foreach (var piece in blackPieces)
//        {
//            foreach (var move in GetLegalMoves(piece))
//            {
//                int minDist = whitePieces.Min(w => ManhattanDistance(move.to, w.CurrentPosition));
//                candidates.Add((piece.Id, piece.CurrentPosition, move.to, move.isCapture, minDist));
//            }
//        }

//        if (!candidates.Any()) return null;

//        var captures = candidates.Where(c => c.isCapture).ToList();
//        var best = captures.Any()
//            ? captures.OrderBy(c => c.dist).First()
//            : candidates.OrderBy(c => c.dist).First();

//        return AIMovement.Success(best.pieceId, best.from, best.to);
//    }
//    #endregion

//    #region 工具方法
//    /// <summary>
//    /// team 陣營目前活著的棋子。統一在這裡過濾 IsCaptured,見檔頭說明。
//    /// </summary>
//    private List<ChessPiece> GetAlivePieces(Team team)
//        => _queryService.GetPiecesForTeam(team).Where(p => !p.IsCaptured).ToList();

//    /// <summary>
//    /// 在 GetAlivePieces 基礎上,依場景設定決定 King 是否可被 AI 選為
//    /// 獵手/移動者/執行捕獲的棋子(對應舊版 MGFFallbackAI 排除 King 的行為)。
//    /// </summary>
//    private List<ChessPiece> GetMovablePool(Team team)
//    {
//        var alive = GetAlivePieces(team);
//        return _excludeKingFromMoves
//            ? alive.Where(p => p.Type != ChessPieceType.King).ToList()
//            : alive;
//    }

//    private List<(PiecePosition to, bool isCapture)> GetLegalMoves(ChessPiece piece)
//    {
//        var moves = new List<(PiecePosition to, bool isCapture)>();
//        var dims = _queryService.Dimensions;

//        for (int x = 0; x < dims.BoardSizeX; x++)
//        {
//            for (int y = 0; y < dims.BoardSizeY; y++)
//            {
//                var pos = new PiecePosition(x, y);
//                if (!_validator.IsValidMove(piece.Id, pos)) continue;

//                var occupant = _queryService.GetPieceAt(pos);
//                bool isCapture = occupant != null && !occupant.IsCaptured && occupant.Team != piece.Team;
//                moves.Add((pos, isCapture));
//            }
//        }
//        return moves;
//    }

//    private static int ManhattanDistance(PiecePosition a, PiecePosition b)
//        => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
//    #endregion
//}
