using System;

public class PieceID : IEquatable<PieceID>
{
    public string Value { get; }
    private PieceID(string value) => Value = value ?? Guid.NewGuid().ToString();
    public static PieceID Generate() => new(Guid.NewGuid().ToString());
    public override string ToString() => Value;
    public static PieceID FromString(string value) => new(value);   // 從字串創建 PieceID


    public override bool Equals(object obj) => obj is PieceID other && Equals(other);
    public bool Equals(PieceID other)
    {
        if (other == null) return false;  // 防 NRE
        if (Value == null) return false;  // Value 也防護
        return Value == other.Value;
    }
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;


    // 操作符重載（== 和 !=）
    public static bool operator ==(PieceID left, PieceID right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }
    public static bool operator !=(PieceID left, PieceID right) => !(left == right);
}
