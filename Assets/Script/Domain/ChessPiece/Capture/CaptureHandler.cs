using UnityEngine;
using Zenject;

#region 介面&結構
public interface ICaptureHandler
{
    CaptureResult HandleCapture(
    ChessPiece capturedPiece,
    ChessPiece capturer,
    PiecePosition position);
}

/// <summary>
/// 捕獲結果
/// </summary>
public struct CaptureResult
{
    public bool Success;
    public CaptureMode Mode;
    public string Message;

    public static CaptureResult Succeeded(CaptureMode mode, string message = "")
    {
        return new CaptureResult { Success = true, Mode = mode, Message = message };
    }

    public static CaptureResult Failed(string reason)
    {
        return new CaptureResult { Success = false, Message = reason };
    }
}
#endregion

#region 邏輯實作(Domain Layer)
/// <summary>
/// 永久移除：標準象棋規則
/// </summary>
public class RemovalCaptureHandler : ICaptureHandler
{
    public CaptureResult HandleCapture(
       ChessPiece capturedPiece,
       ChessPiece capturer,
       PiecePosition position)
    {
        Debug.Log($"[PermanentRemoval] {capturedPiece.Type}({capturedPiece.Id}) 被永久移除");

        capturedPiece.Capture(CaptureMode.Remove);

        return CaptureResult.Succeeded(
            CaptureMode.Remove,
            $"{capturedPiece.Type} 已被永久移除"
        );
    }
}

/// <summary>
/// 將棋捕獲：轉換陣營並加入手牌
/// </summary>
public class ShogiCaptureHandler : ICaptureHandler
{
    [Inject] private readonly IPieceFactory _pieceFactory;
    [Inject] private readonly IStandbyZone _standbyZone;

    public CaptureResult HandleCapture(
       ChessPiece capturedPiece,
       ChessPiece capturer,
       PiecePosition position)
    {
        Debug.Log($"[StandardCapture] {capturedPiece.Type}({capturedPiece.Id}) 被捕獲，進入待命區");

        // Step 1: 先標記為 Removed（從棋盤移除）
        capturedPiece.Capture(CaptureMode.ToStandby);

        // Step 2: 創建轉換後的棋子（對方陣營）
        Team newTeam = capturer.Team;
        capturedPiece.ConvertToTeam(newTeam);

        // Step 3: 加入攻擊方待命區
        _standbyZone.AddPiece(capturedPiece);

        var resultMsg = $"{capturedPiece.Type} 轉為 {capturer.Team} 方棋子，可重新部署";
        Debug.Log($"[ShogiCapture] {resultMsg}");
        return CaptureResult.Succeeded(CaptureMode.ToStandby, resultMsg);
    }
}
#endregion