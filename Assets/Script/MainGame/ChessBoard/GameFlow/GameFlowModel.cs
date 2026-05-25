using UnityEngine;

public class GameFlowModel
{
    // 記錄是否有一方陷入無合法步的狀態 (Checkmate / Stalemate)
    public bool IsEnemyStuck { get; set; } = false;
    public bool IsPlayerStuck { get; set; } = false;

    public void Reset()
    {
        IsEnemyStuck = false;
        IsPlayerStuck = false;
    }
}
