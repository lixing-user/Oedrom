using UnityEngine;

// Domain/ValueObjects/ResourceModifier.cs
/// <summary>
/// 資源修改器（Buff/Debuff 效果）
/// </summary>
public class ResourceModifier
{
    public string EffectId { get; }
    public VariableType TargetResource { get; }
    public float FlatAmount { get; }      // 固定數值
    public float PercentAmount { get; }   // 百分比
    public int RemainingTurns { get; private set; }

    public ResourceModifier(
        string effectId,
        VariableType targetResource,
        float flatAmount,
        float percentAmount,
        int duration)
    {
        EffectId = effectId;
        TargetResource = targetResource;
        FlatAmount = flatAmount;
        PercentAmount = percentAmount;
        RemainingTurns = duration;
    }


    /// <summary>
    /// 計算修改後的數值
    /// </summary>
    public float Apply(float baseValue)
    {
        return (baseValue + FlatAmount) * (1f + PercentAmount);        // 先加固定值，再乘百分比
    }

    public void DecrementDuration()
    {
        if (RemainingTurns > 0)
            RemainingTurns--;
    }

    public bool IsExpired => RemainingTurns <= 0;
}
