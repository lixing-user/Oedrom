using UnityEngine;

public class Battle
{
    public BattleId Id { get; private set; }
    public ChessPiece Attacker { get; private set; }
    public ChessPiece Defender { get; private set; }
    public BattleStatus Status { get; private set; }
    public BattleResult Result { get; private set; }

}


// Domain / Battle / BattleResult.cs（值對象）
public class BattleResult
{
    public Team Winner { get; }
    public int BattleCount { get; }
    public bool AttackerSurvived { get; }
    public bool DefenderSurvived { get; }

    public BattleResult(Team winner, int battleCount, ChessPiece attacker, ChessPiece defender)
    {
        Winner = winner;
        BattleCount = battleCount;
        AttackerSurvived = winner == attacker.Team;
        DefenderSurvived = winner == defender.Team;
    }
}

public enum BattleStatus
{
    Pending,      // 待開始
    InProgress,   // 進行中
    Completed     // 已完成
}