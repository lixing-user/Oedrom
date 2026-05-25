using UnityEngine;

// 自動放置棋子的Result Pattern
public class DeploymentResult
{
    public bool Success { get; set; }
    public int PlacedCount { get; set; }
    public string ErrorMessage { get; set; }

    public DeploymentErrorType? ErrorType { get; set; } // 可選：錯誤類型枚舉
}

public enum DeploymentErrorType
{
    EmptyHand,
    BoardFull,
    InvalidPosition,
    InsufficientResources,
    InsufficientMorale
}


// 清除棋子返回手牌的Result Pattern
public class ClearResult
{
    public bool Success { get; set; }
    public int ClearedCount { get; set; }
}