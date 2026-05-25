using System;
using UnityEngine;

#region Battle Lifecycle Signal
//Battle Session遊戲開始專用信號
public class GameStartedSignal
{
    public Team InitialTurn { get; }
    public string SceneName { get; }
    public DateTime StartTime { get; }

    public GameStartedSignal(
        Team initialTurn,
        string sceneName)
    {
        InitialTurn = initialTurn;
        SceneName = sceneName;
        StartTime = DateTime.Now;
    }
}

public class GameEndedSignal
{
    public Team WinnerTeam { get; }
    public string Reason { get; }  // 如 "對手王已被吃掉"

    public GameEndedSignal(Team winnerTeam, string reason)
    {
        WinnerTeam = winnerTeam;
        Reason = reason;
    }
}

// 請求顯示戰鬥獎勵畫面的 Signal
public struct ShowRewardRequestedSignal { }

// 請求顯示Chase Battle畫面的 Signal
public struct ShowChaseBattleRequestedSignal { }
#endregion