using Unity.VisualScripting;
using UnityEngine;
using ChessPieceCardProduction;

// Infrastructure/Persistence/DTOs/ChessPieceDTO
[System.Serializable]
public class PieceDTO
{
    public string Pieceid;  // 棋子ID記錄
    public string CardId;   // 棋子所屬來源卡牌ID記錄
    public ChessPieceType type;
    public int team;
    public int currentX;
    public int currentY;
    public bool hasMoved;
    public bool isCaptured;
    public int state;

    // 空構造函數（用於反序列化）
    public PieceDTO() { }

    public PieceDTO(ChessPiece piece)
    {
        Pieceid = piece.Id.Value;
        CardId = piece.SourceCardId?.Value;
        type = piece.Type;
        team = (int)piece.Team;
        currentX = piece.CurrentPosition.X;
        currentY = piece.CurrentPosition.Y;
        hasMoved = piece.HasMoved;
        isCaptured = piece.IsCaptured;
        state = (int)piece.State;
    }

    // 轉換為 Domain Entity（用於讀檔）
    public ChessPiece ToDomain(IMovementStrategyFactory strategyFactory)
    {
        var pieceId = PieceID.FromString(Pieceid);
        var cardId = string.IsNullOrEmpty(CardId) ? null : CardID.FromString(CardId);
        var position = new PiecePosition(currentX, currentY);
        var strategy = strategyFactory.Create(type);

        var piece = new ChessPiece(
            pieceId,
            cardId,
            type,
            (Team)team,
            position,
            strategy
        );

        // 恢復運行時狀態
        if (hasMoved)
            piece.MoveTo(position);  // 觸發 HasMoved = true
        if (isCaptured)
            piece.Capture();

        return piece;
    }
}
