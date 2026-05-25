using UnityEngine;

#region TutorialProgressSignal
//public struct StartTutorialSignal { }
//public struct TutorialCompletedSignal { }
// 新增一個信號，用來同場景傳遞要播放的步驟
public struct PlayTutorialSequenceSignal
{
    public string SequenceId;
    public TutorialStep[] Steps;
}

//// 信號：播放完畢
//public struct TutorialSequenceCompletedSignal
//{
//    public string SequenceId;
//}
#endregion

#region Session Signals
// ── 命令 Signal（外部 → StateMachine）──
// 任何人想切換狀態，只要 Fire 這個，不需要持有 StateMachine 的引用
public class RequestSessionStateChangeSignal
{
    public GameSessionState TargetState { get; set; }
}

// ── 結果 Signal（StateMachine → 所有人）──
// 狀態真正切換後廣播，讓 HUD/存檔/音效等系統自行反應
public class GameSessionStateChangedSignal
{
    public GameSessionState PreviousState { get; set; }
    public GameSessionState NextState { get; set; }
}


// 由 Lobby UI 發出。代表「玩家想要開始遊戲了，請準備好數據」
public class StartGameCommandSignal
{
    public GameStartMode Mode;
}

//從其他場景切回到MainGameField時，被動接收並繼續處理Phase流程
public class SceneReactivatedSignal
{
    public string FromScene { get; }

    public SceneReactivatedSignal(string fromScene)
    {
        FromScene = fromScene;
    }
}
#endregion

#region Battle Signals
public class RequestEnterBattleSignal
{
    public BattleType BattleType;
    //public PendingPieceBattleMove PendingBattle { get; set; }
    public BattleSessionData PendingBattle { get; set; }
}

public class ChangeBattleTypeSignal
{
    public BattleType NewBattleType;  // Normal 》 Chase, Final 》 Escape , etc.
}
#endregion

#region Shop Signals
public class RequestEnterShopSignal {}
#endregion
