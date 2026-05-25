using UnityEngine;

public class BattleId
{
    public string Value { get; }
    private BattleId(string value) => Value = value ?? System.Guid.NewGuid().ToString();
    public static BattleId Generate() => new(System.Guid.NewGuid().ToString());
    public override string ToString() => Value;
    public static BattleId FromString(string value) => new(value);   // ±q¦r¦ê³Ð«Ø BattleId
}
