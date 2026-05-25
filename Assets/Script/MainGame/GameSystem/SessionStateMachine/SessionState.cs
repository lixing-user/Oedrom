using Systems.SceneManagement;
using UnityEngine;
using Zenject;

#region Lobby
public enum GameStartMode
{
    NewGame,    // 從頭開始（重置進度，跑 Prologue）
    LoadGame,   // 從存檔恢復（讀取進度，回到上次離開的 Session）
    DebugStart  // 開發測試（直接跳過劇情進入特定場景）
}

public class MainMenuSessionState : IGameSessionState
{
    public void OnEnter(GameSessionStateMachine machine)
    {
        if (SceneController.Instance == null)
        {
            Debug.Log("[MainMenuSession] SceneController 不存在，跳過場景切換（單場景測試模式）");
            return;
        }

        Debug.Log("[MainMenuSessionState] 進入主選單");
    }

    public void OnExit(GameSessionStateMachine machine) 
    {
        Debug.Log("[MainMenuSessionState] 離開主選單");
    }
}
#endregion


#region Story
public class StorySessionState : IGameSessionState
{
    public void OnEnter(GameSessionStateMachine machine)
    {
        if (SceneController.Instance == null)
        {
            Debug.Log("[StorySessionState] SceneController 不存在，跳過場景切換（單場景測試模式）");
            return;
        }
        Debug.Log("[StorySessionState] 進入故事模式");
    }
    public void OnExit(GameSessionStateMachine machine) 
    { 
        Debug.Log("[StorySessionState] 離開故事模式");
    }
}
#endregion


#region Main Game Field
public class MainGameSessionState : IGameSessionState
{
    public void OnEnter(GameSessionStateMachine machine)
    {
        if (SceneController.Instance == null)
        {
            Debug.Log("[MainGameSessionState] SceneController 不存在，跳過場景切換（單場景測試模式）");
            return;
        }

        Debug.Log("[MainGameSessionState] 進入主要遊戲場景");

        var cameraPresenter = Object.FindAnyObjectByType<CameraPresenter>();
        if (cameraPresenter != null)
        {
            cameraPresenter.PlayOpeningSequence();
        }
    }

    public void OnExit(GameSessionStateMachine machine) 
    {
        Debug.Log("[MainGameSessionState] 離開主要遊戲場景");
    }
}
#endregion


#region Battle Session States
public enum BattleType
{
    Demonstration,    // 示範戰鬥（教學用）
    Normal,
    Chase,
    Escape,
    Loot,
    Event,
    Final
}
public class BattleSessionState : IGameSessionState
{
    private BattleType _currentBattleType = BattleType.Normal;
    public BattleType CurrentBattleType => _currentBattleType;

    public void OnEnter(GameSessionStateMachine machine)
    {
        Debug.Log("[BattleSessionState] 進入戰鬥 Session");
    }

    public void OnExit(GameSessionStateMachine machine)
    {
        Debug.Log("[BattleSessionState] 離開戰鬥 Session");
    }

    public void SetBattleType(BattleType battleType)
    {
        _currentBattleType = battleType;
    }
}
#endregion


#region Shop Session State
public class ShopSessionState : IGameSessionState
{
    public void OnEnter(GameSessionStateMachine machine)
    {
        Debug.Log("[ShopState] 進入商店");

        if (SceneController.Instance == null)
        {
            Debug.Log("[BattleSessionState] SceneController 不存在，跳過場景切換（單場景測試模式）");
            return;
        }

        //SceneController.Instance
        //    .NewTransition()
        //    .HideMainScene(SceneName.MainGameField)
        //    .Load(Slot.SessionContent, SceneName.Shop)
        //    .WithOverlay()
        //    .Perform();
    }

    public void OnExit(GameSessionStateMachine machine)
    {
        Debug.Log("[ShopState] 離開商店");

        //SceneController.Instance
        //    .NewTransition()
        //    .Unload(Slot.SessionContent)
        //    .ShowMainScene(SceneName.MainGameField)
        //    .WithOverlay()
        //    .Perform();
    }
}
#endregion


#region Ending 
public class EndingSessionState : IGameSessionState
{
    public void OnEnter(GameSessionStateMachine machine)
    {
        Debug.Log("[EndingSessionState] 進入結尾場景");

        if (SceneController.Instance == null)
        {
            Debug.Log("[EndingSessionState] SceneController 不存在，跳過場景切換（單場景測試模式）");
            return;
        }
    }

    public void OnExit(GameSessionStateMachine machine)
    {
        Debug.Log("[EndingSessionState] 離開結尾場景");
    }
}
#endregion