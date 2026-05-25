using UnityEngine;

// 在持久層建立一個跨場景的資料容器
public class BattleContextHolder
{
    public BattleSessionData CurrentBattle { get; private set; }
    public Team? BattleWinner { get; private set; }

    public void SetBattle(BattleSessionData data)
    {
        CurrentBattle = data;
        BattleWinner = null;
        Debug.Log($"[BattleContextHolder] SetBattle 被呼叫: " +
                  $"Attacker={data?.AttackerType}, Defender={data?.DefenderType}");
    }

    public void SetBattleResult(Team winner)
    {
        BattleWinner = winner;
        Debug.Log($"[BattleContextHolder] 戰鬥結果：{winner} 勝出");
    }

    public void ResetBattleResult()
    {
        BattleWinner = null;
        Debug.Log("[BattleContextHolder] 戰鬥結果已重置 (用於教學/時光倒流)");
    }

    public void Clear()
    {
        CurrentBattle = null;
        BattleWinner = null;
    }
}
