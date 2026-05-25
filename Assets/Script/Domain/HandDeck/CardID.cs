using System;
using UnityEngine;

namespace ChessPieceCardProduction
{
    public class CardID : IEquatable<CardID>
    {
        public string Value { get; }

        public static readonly CardID Empty = new CardID(string.Empty); //空值實例（避免使用 null）
        private CardID(string value) => Value = value ?? Guid.NewGuid().ToString();
        public static CardID Generate() => new(Guid.NewGuid().ToString());
        public override string ToString() => Value;
        public static CardID FromString(string value) => new(value);   // 從字串創建 CardID

        public override bool Equals(object obj) => obj is CardID other && Equals(other);
        public bool Equals(CardID other)
        {
            if (other == null) return false;  // 防 NRE
            if (Value == null) return false;  // Value 也防護
            return Value == other.Value;
        }
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
        public bool IsEmpty => string.IsNullOrEmpty(Value);


        // 操作符重載（== 和 !=）
        public static bool operator ==(CardID left, CardID right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }
        public static bool operator !=(CardID left, CardID right) => !(left == right);
    }
}
