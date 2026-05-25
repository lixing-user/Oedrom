using UnityEngine;

[CreateAssetMenu(menuName = "QuestSystem/Actions/Macro/WatchTileMovement")]
public class MacroAction_WatchTileMovement : MacroActionSO
{
    [Header("要監聽的目標位置")]
    public Vector2Int TargetPosition;

    [Header("要監聽的目標隊伍 (留空表示監聽所有隊伍)")]
    public Team TargetTeam; 

    [Header("達成時要寫入的 Flag (用來通知 QuestManager 任務完成)")]
    public string FlagToSetOnComplete;

    public override void Execute(ProjectGameActionContext context)
    {
        // 為了跨場景監聽，我們動態建立一個不可見的 GameObject 來掛載監聽腳本
        // 這樣即使切換場景，監聽器依然存活
        GameObject watcherObj = new GameObject($"QuestWatcher_Tile_{TargetPosition}");
        DontDestroyOnLoad(watcherObj);

        var watcher = watcherObj.AddComponent<TileMovementWatcher>();
        // 將 Flag 參數傳遞給 Watcher
        watcher.Initialize(context, TargetPosition, TargetTeam, FlagToSetOnComplete);
    }
}