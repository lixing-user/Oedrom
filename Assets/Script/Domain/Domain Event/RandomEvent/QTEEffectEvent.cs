using System;
using UnityEngine;

#region 
public class TurnEffectsAppliedDomainEvent : DomainEvent
{
    public DateTime OccurredAt { get; }

    public TurnEffectsAppliedDomainEvent()
    {
        OccurredAt = DateTime.UtcNow;
    }
}
#endregion

#region Modifier Effect
public class ModifierAppliedDomainEvent : DomainEvent
{
    public string EffectId { get; }
    public VariableType TargetResource { get; }
    public int Duration { get; }
    public ModifierAppliedDomainEvent(string effectId, VariableType targetResource, int duration)
    {
        EffectId = effectId;
        TargetResource = targetResource;
        Duration = duration;
    }
}

public class ModifierRemovedDomainEvent : DomainEvent
{
    public string EffectId { get; }
    public ModifierRemovedDomainEvent(string effectId)
    {
        EffectId = effectId;
    }
}
#endregion