using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件效果配置（Infrastructure → Domain 的橋接）
/// </summary>
[System.Serializable]
public class EventEffectConfig
{
    [Header("核心設定")]
    public EventEffectType effectType;


    [AllowNesting]
    [BoxGroup("目標與來源")]
    [ShowIf(EConditionOperator.Or, "IsResourceRelated", "IsDynamicRelated")]
    [Label("目標資源類型")]
    public VariableType targetResource;


    [AllowNesting]
    [BoxGroup("目標與來源")]
    [ShowIf("effectType", EventEffectType.DynamicResourceChange)]
    [Label("參考來源資源")]
    public VariableType sourceResource;


    [AllowNesting]
    [BoxGroup("棋子與牌組")]
    [ShowIf("effectType", EventEffectType.DeckEffect)]
    public ChessPieceType targetPieceType; // 獲得的棋子類型 (若為 None 代表隨機)

    [AllowNesting]
    [BoxGroup("棋子與牌組")]
    [ShowIf("ShowRandomPool")]
    [Label("隨機池 (留空為全域隨機)")]
    public RandomPoolWrapper randomPool = new RandomPoolWrapper();

    [AllowNesting]
    [BoxGroup("棋子與牌組")]
    [ShowIf("effectType", EventEffectType.DeckEffect)]
    public Team targetTeam = Team.White;


    [AllowNesting]
    [BoxGroup("戰鬥觸發")]
    [ShowIf("effectType", EventEffectType.TriggerBattle)]
    public BattleType targetBattleType = BattleType.Normal;

    [AllowNesting]
    [BoxGroup("戰鬥觸發")]
    [ShowIf("effectType", EventEffectType.TriggerBattle)]
    [Label("戰鬥關卡/敵人陣容 ID")]
    public string targetBattleId;


    [AllowNesting]
    [BoxGroup("戰鬥修正")]
    [ShowIf("effectType", EventEffectType.CombatModifier)]
    public CombatModifierType combatModifierType;


    [AllowNesting]
    [BoxGroup("狀態異常")]
    [ShowIf("effectType", EventEffectType.StatusEffect)]
    [Label("狀態類型")]
    public StatusEffectType statusType;


    [AllowNesting]
    [BoxGroup("數值與時效")]
    [ShowIf("IsValueNeeded")]
    [Label("固定值 (Flat)")]
    public float flatAmount; // 固定數值

    [AllowNesting]
    [BoxGroup("數值與時效")]
    [ShowIf("IsValueNeeded")]
    [Label("係數/百分比 (Scale)")]
    public float percentAmount; // 百分比係數

    [AllowNesting]
    [BoxGroup("數值與時效")]
    [ShowIf("IsDurationNeeded")]
    [Label("持續回合 (1 為即時)")]
    public int duration = 1; // 持續回合數（1 = 即時效果）

    #region 邏輯判斷 (Helper Properties for NaughtyAttributes)
    private bool IsResourceRelated => effectType == EventEffectType.ResourceChange || effectType == EventEffectType.ResourceModifier;
    private bool IsDynamicRelated => effectType == EventEffectType.DynamicResourceChange;
    private bool IsValueNeeded => effectType != EventEffectType.TriggerBattle ;
    private bool IsDurationNeeded => effectType == EventEffectType.ResourceModifier || effectType == EventEffectType.CombatModifier || effectType == EventEffectType.StatusEffect;
    private bool ShowRandomPool => effectType == EventEffectType.DeckEffect && targetPieceType == ChessPieceType.None;
    #endregion
}

/// <summary>
/// 條件效果配置
/// </summary>
[System.Serializable]
public class EventConditionConfig
{
    public ConditionType conditionType;
    public float threshold; // 觸發門檻值
}


public enum EventEffectType  //Enum 新增值時，請加在最後面，避免打亂已序列化的 SO 數值
{
    ResourceChange,      // 即時資源變化
    ResourceModifier,    // 持續效果（Buff/Debuff）
    TriggerBattle,      // 觸發戰鬥
    StatusEffect,        // 狀態效果
    ConditionalEffect,    // 條件效果
    DeckEffect,          // 牌組效果
    DynamicResourceChange,   // 動態公式資源變化
    CombatModifier,      // 戰鬥修正（影響戰鬥結果的效果）
}

public enum ConditionType
{
    MoraleGreaterThan,    // 軍心大於
    MoraleLessThan,       // 軍心小於
    PopularityGreaterThan,// 民心大於
    PopularityLessThan,   // 民心小於
    HandCardCountGreaterThan // 手牌數大於 (用於內鬥、叛變)
}

public enum StatusEffectType
{
    None,
    Stun,       // 暈眩 (無法行動)
    Plague,     // 瘟疫 (持續掉血/機率陣亡)
    Attrition   // 消耗戰 (無法行動，解除時觸發戰鬥)
}

[System.Serializable]
public class RandomPoolWrapper
{
    // 這裡就不需要 ShowIf 了，讓它乖乖顯示
    [AllowNesting]
    public List<ChessPieceType> pieces = new List<ChessPieceType>();
}