using System;
using Systems.SceneManagement;
using UnityEngine;
using Zenject;

#region Turn Signal
/// <summary>
/// === 場景級 Signal：用於場景內狀態機通訊 ===
/// </summary>
public class TurnChangedSignal
{
    public Team CurrentTurn { get; }
    public Team PreviousTurn { get; }

    public TurnChangedSignal(Team currentTurn, Team previousTurn)
    {
        CurrentTurn = currentTurn;
        PreviousTurn = previousTurn;
    }
}


/// <summary>
/// === 全域級 Signal：用於跨場景的持久化系統 ===
/// </summary>
public class GlobalTurnChangedSignal
{
    public Team CurrentTurn { get; }
    public int TurnNumber { get; }
    public string SceneName { get; }

    public GlobalTurnChangedSignal(Team currentTurn, int turnNumber, string sceneName)
    {
        CurrentTurn = currentTurn;
        TurnNumber = turnNumber;
        SceneName = sceneName;
    }
}


/// <summary>
/// 回合結束信號
/// </summary>
public class TurnEndedSignal
{
    public Team Team { get; }

    public TurnEndedSignal(Team team)
    {
        Team = team;
    }
}
#endregion

#region Flow Signal
public class FlowChangedSignal
{
    public GameFlow CurrentFlow { get; }
    public GameFlow PreviousFlow { get; }
    public FlowChangedSignal(GameFlow currentFlow, GameFlow previousFlow)
    {
        CurrentFlow = currentFlow;
        PreviousFlow = previousFlow;
    }
}
#endregion

#region Phase Signal
public class PhaseChangedSignal
{
    public GamePhaseState CurrentPhase { get; }
    public GamePhaseState PreviousPhase { get; }
    public PhaseChangedSignal(GamePhaseState currentPhase, GamePhaseState previousPhase)
    {
        CurrentPhase = currentPhase;
        PreviousPhase = previousPhase;
    }
}

public class PhaseEndRequestedSignal
{
    public string RequestSource { get; }  // "Button" 或 "Hotkey"

    public PhaseEndRequestedSignal(string requestSource = "Button")
    {
        RequestSource = requestSource;
    }
}


public class ResolutionTriggeredSignal
{
    public ResolutionType Type { get; }

    public ResolutionTriggeredSignal(ResolutionType type)
    {
        Type = type;
    }
}
#endregion