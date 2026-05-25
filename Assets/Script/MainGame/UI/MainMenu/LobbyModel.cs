using UnityEngine;
using UnityEngine.UI;

public enum LobbyState
{
    MainMenu,
    StartGame,
    Library,
    Quit
}

//遊戲流程協調 DDD _ Model
public class LobbyModel
{
    public LobbyState CurrentState { get; private set; } = LobbyState.MainMenu;

    public event System.Action<LobbyState> OnStateChanged;

    public void SetState(LobbyState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }
}
