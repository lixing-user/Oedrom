using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Condition/Micro/BattleEnded")]
public class BattleSessionEndedCondition : MicroConditionSO
{
    [Tooltip("指定誰獲勝才算條件達成。設為 None 代表『不管誰贏，只要遊戲結束就算達成』")]
    public Team RequiredWinner = Team.None;

    // 用來記錄 Signal 是否已經被觸發
    private bool _hasGameEnded = false;


    public override void OnStart(SceneGameActionContext context)
    {
        // 【關鍵】ScriptableObject 在 Unity Editor 中不會自動重置狀態
        // 所以每次步驟開始時，務必將布林值重置為 false
        _hasGameEnded = false;

        // 訂閱遊戲結束信號
        context.SignalBus.Subscribe<GameEndedSignal>(OnGameEnded);
    }

    private void OnGameEnded(GameEndedSignal signal)
    {
        // 如果 RequiredWinner 是 None，代表我們只在乎遊戲有沒有結束
        if (RequiredWinner == Team.None)
        {
            _hasGameEnded = true;
        }
        // 如果有指定獲勝者，則需比對 Signal 傳來的獲勝隊伍
        else if (signal.WinnerTeam == RequiredWinner)
        {
            _hasGameEnded = true;
        }
    }

    public override bool IsConditionMet(SceneGameActionContext context)
    {
        // 讓 WaitUntil 每幀來檢查這個值
        return _hasGameEnded;
    }

    public override void OnExit(SceneGameActionContext context)
    {
        // 【關鍵】步驟結束或中斷時，務必取消訂閱，防止殘留的監聽器引發 NullReferenceException 或記憶體洩漏
        context.SignalBus.TryUnsubscribe<GameEndedSignal>(OnGameEnded);
    }
}
