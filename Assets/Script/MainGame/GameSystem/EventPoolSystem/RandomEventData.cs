using System.Collections.Generic;
using UnityEngine;


public enum RandomEventType
{
    Ally,
    Enemy,
    Neutral
}


[CreateAssetMenu(fileName = "RandomEventData", menuName = "Scriptable Objects/RandomEventData")]
public class RandomEventData : ScriptableObject
{
    [Header("基礎資訊")]
    public string eventId;
    public string eventName;

    [TextArea(3, 5)]
    public string description;
    [Min(1)]
    public int weight = 1; // 權重值，用於加權隨機
    public Sprite eventIcon;
    public RandomEventType eventType;


    [Header("觸發前置條件 (若為空則必定進池)")]
    public List<EventConditionConfig> conditions = new List<EventConditionConfig>();

    [Header("事件選項")]
    public List<EventOption> options = new List<EventOption>();


    #region Evaluate Conditions
    public bool CheckConditions(IEffectContext context)
    {
        // 如果沒有設定條件，代表無條件可以觸發
        if (conditions == null || conditions.Count == 0) return true;

        // 檢查所有條件 (AND 邏輯：必須全部滿足才算通過)
        foreach (var condition in conditions)
        {
            if (!EvaluateCondition(condition, context))
            {
                return false;
            }
        }
        return true;
    }

    private bool EvaluateCondition(EventConditionConfig config, IEffectContext context)
    {
        switch (config.conditionType)
        {
            case ConditionType.MoraleGreaterThan:
                return context.PlayerResource.Morale > config.threshold;

            case ConditionType.MoraleLessThan:
                return context.PlayerResource.Morale < config.threshold;

            case ConditionType.PopularityGreaterThan:
                return context.PlayerResource.Popularity > config.threshold;

            case ConditionType.PopularityLessThan:
                return context.PlayerResource.Popularity < config.threshold;

            case ConditionType.HandCardCountGreaterThan:
                return context.PlayerHand.Cards.Count > config.threshold;

            default:
                Debug.LogWarning($"[EventCondition] 未實作的條件判斷: {config.conditionType}");
                return true; // 容錯處理
        }
    }
    #endregion
}


[System.Serializable]
public class EventOption
{
    public string optionId;
    public string optionName; // 顯示在 UI 按鈕上的文字，例如 "迎戰"
    public string description; // 選項的具體描述
    public List<EventEffectConfig> effects = new List<EventEffectConfig>();

    // 未來擴充保留：可以加入條件判斷 (例如：身上有特定道具才能選)
    // public List<EventConditionConfig> requirements; 
}


