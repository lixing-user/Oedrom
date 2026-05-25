using System;
using UnityEngine;

public enum AIMovementStatus
{
    Success,              // 正常執行成功
    FallbackUsed,         // 使用備用方案（隨機移動）
    ParseError,           // UCI 解析錯誤
    OutOfBounds,          // 座標超出範圍
    InvalidMove,          // 違反領域規則
    EngineError,          // 引擎異常
    NoLegalMoves          // 無可用移動
}

public class AIMovement
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }
    public AIMovementStatus Status { get; }

    // 成功時的數據
    public PieceID PieceId { get; }
    public PiecePosition From { get; }
    public PiecePosition To { get; }

    private AIMovement(
        bool isSuccess,
        AIMovementStatus status,
        string errorMessage = null,
        PieceID pieceId = null,
        PiecePosition from = null,
        PiecePosition to = null)
    {
        IsSuccess = isSuccess;
        Status = status;
        ErrorMessage = errorMessage ?? string.Empty;
        PieceId = pieceId;
        From = from;
        To = to;
    }

    #region 工廠方法（靜態建構器）

    // 成功情況
    public static AIMovement Success(PieceID pieceId, PiecePosition from, PiecePosition to)
    {
        return new AIMovement(
            isSuccess: true,
            status: AIMovementStatus.Success,
            pieceId: pieceId,
            from: from,
            to: to
        );
    }

    // 使用備用方案（隨機移動）
    public static AIMovement FallbackUsed(PieceID pieceId, PiecePosition from, PiecePosition to)
    {
        return new AIMovement(
            isSuccess: true,
            status: AIMovementStatus.FallbackUsed,
            errorMessage: "AI 引擎無回應，使用隨機移動",
            pieceId: pieceId,
            from: from,
            to: to
        );
    }

    // 協議解析失敗
    public static AIMovement ParseError(string uciMove, string reason)
    {
        return new AIMovement(
            isSuccess: false,
            status: AIMovementStatus.ParseError,
            errorMessage: $"UCI 解析失敗: {uciMove} - {reason}"
        );
    }

    // 移動超出棋盤範圍
    public static AIMovement OutOfBounds(string uciMove)
    {
        return new AIMovement(
            isSuccess: false,
            status: AIMovementStatus.OutOfBounds,
            errorMessage: $"移動 {uciMove} 超出棋盤範圍"
        );
    }

    // 領域驗證失敗（違反業務規則）
    public static AIMovement DomainValidationFailed(string reason)
    {
        return new AIMovement(
            isSuccess: false,
            status: AIMovementStatus.InvalidMove,
            errorMessage: $"領域驗證失敗: {reason}"
        );
    }

    // 引擎異常
    public static AIMovement EngineError(Exception ex)
    {
        return new AIMovement(
            isSuccess: false,
            status: AIMovementStatus.EngineError,
            errorMessage: $"引擎異常: {ex.Message}"
        );
    }

    // 無合法移動
    public static AIMovement NoLegalMoves()
    {
        return new AIMovement(
            isSuccess: false,
            status: AIMovementStatus.NoLegalMoves,
            errorMessage: "黑方無任何合法移動"
        );
    }

    #endregion
}
