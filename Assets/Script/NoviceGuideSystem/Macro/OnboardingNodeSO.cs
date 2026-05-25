using UnityEngine;

public enum QuestType { StoryMain, ExplorationMain, Side }

//全域 (Global)。跨場景存在，隨時都在背景監聽。
[CreateAssetMenu(fileName = "Quest", menuName = "QuestSystem/QuestSO")]
public class QuestSO : ScriptableObject
{
    public string questId;
    public QuestType questType;
    //public bool isMainQuest; // 判斷是否為影響全局流程的主線

    [Header("解鎖與觸發條件")]
    public MacroConditionSO[] unlockConditions;

    [Header("觸發時立即執行的動作 (例如：鎖定畫面、播劇情)")]
    public MacroActionSO[] actionsOnStart;

    [Header("推進與完成條件")]
    public MacroConditionSO[] completionConditions;

    [Header("完成時執行的動作 (例如：解鎖畫面、發放獎勵)")]
    public MacroActionSO[] actionsOnComplete;
}