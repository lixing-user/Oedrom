using UnityEngine;

/// <summary>
/// 定義棋子狀態的基本屬性
/// </summary>
public interface IStatusEffect
{
    string StatusId { get; }
    string Name { get; }
    string Description { get; }
    Sprite Icon { get; }
    int RemainingTurns { get; }

    void OnApply(ChessPiece target);    // 掛載時觸發 (例如: 鎖定棋子)
    void OnRemove(ChessPiece target);   // 移除時觸發 (例如: 解鎖棋子)
    bool OnTurnTick(ChessPiece target); // 每回合觸發 (扣減回合數)
}



// Domain/Statuses/StunStatus.cs
public class StunStatus : IStatusEffect
{
    public string StatusId => "status_stun";
    public string Name => "無法行動";
    public string Description => "該棋子無法進行移動與攻擊";
    public Sprite Icon { get; } // 實務上可以透過資源庫/建構子傳入

    public int RemainingTurns { get; private set; }

    public StunStatus(int duration)
    {
        RemainingTurns = duration;
    }

    // 掛載時：呼叫棋子本身的 Lock 邏輯
    public void OnApply(ChessPiece target)
    {
        target.Lock();
        Debug.Log($"[StunStatus] {target.Type} 被附加了暈眩狀態，持續 {RemainingTurns} 回合。");
    }

    // 移除時：解鎖棋子
    public void OnRemove(ChessPiece target)
    {
        target.Unlock();
        Debug.Log($"[StunStatus] {target.Type} 的暈眩狀態解除了。");
    }

    // 每回合結算：扣除回合數，歸零則通知棋子移除
    public bool OnTurnTick(ChessPiece target)
    {
        RemainingTurns--;
        return RemainingTurns <= 0; // 回傳 true 代表已過期
    }
}