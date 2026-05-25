using UnityEngine;

public class ResourceChangedSignal
{
    public VariableType Type { get; }
    public float Value { get; }
    public ResourceChangedSignal(VariableType type, float value) { Type = type; Value = value; }
}

public class EnemyDifficultyDeterminedSignal
{
    public int Popularity { get; }

    public EnemyDifficultyDeterminedSignal(int popularity)
    {
        Popularity = popularity;
    }
}

public class SupplyDepletedWarningSignal
{
    public SupplyDepletedWarningSignal()
    {
    }
}