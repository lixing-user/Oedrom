using System;
using UnityEngine;

public class ResourceInitializedDomainEvent : DomainEvent
{
    public float Morale { get; }
    public float Popularity { get; }
    public float Supply { get; }
    public float Coin { get; }
    public DateTime OccurredAt { get; }

    public ResourceInitializedDomainEvent(float morale, float popularity, float supply, float coin)
    {
        Morale = morale;
        Popularity = popularity;
        Supply = supply;
        Coin = coin;
        OccurredAt = DateTime.UtcNow;
    }
}

public class ResourceResetDomainEvent : DomainEvent
{
    public float DefaultMorale { get; }
    public float DefaultPopularity { get; }
    public float DefaultSupply { get; }
    public float DefaultCoin { get; }
    public DateTime OccurredAt { get; }

    public ResourceResetDomainEvent(float morale, float popularity, float supply, float coin)
    {
        DefaultMorale = morale;
        DefaultPopularity = popularity;
        DefaultSupply = supply;
        DefaultCoin = coin;
        OccurredAt = DateTime.UtcNow;
    }
}

public class ResourceChangedDomainEvent : DomainEvent
{
    public VariableType Type { get; }
    public float NewValue { get; }
    public float OldValue { get; }
    public DateTime OccurredAt { get; }

    public ResourceChangedDomainEvent(VariableType type, float newValue, float oldValue)
    {
        Type = type;
        NewValue = newValue;
        OldValue = oldValue;
        OccurredAt = DateTime.UtcNow;
    }
}

public class PopularityChangedDomainEvent : DomainEvent
{
    public float NewPopularity { get; }
    public DateTime OccurredAt { get; }

    public PopularityChangedDomainEvent(float newPopularity)
    {
        NewPopularity = newPopularity;
        OccurredAt = DateTime.UtcNow;
    }
}

public class  SupplyDepletedDomainEvent : DomainEvent
{
    public DateTime OccurredAt { get; }

    public SupplyDepletedDomainEvent()
    {
        OccurredAt = DateTime.UtcNow;
    }
}