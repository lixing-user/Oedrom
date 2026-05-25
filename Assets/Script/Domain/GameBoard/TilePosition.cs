using System;
using UnityEngine;

// Domain層：用於棋盤格生成、highlight與raycast的位置判斷
public class TilePosition
{
    public int X { get; }
    public int Y { get; }

    public TilePosition(int x, int y)
    {
        X = x;
        Y = y;
    }


    public override string ToString() => $"TilePosition({X}, {Y})";
    public bool Equals(TilePosition other) => X == other.X && Y == other.Y;
    public override int GetHashCode() => HashCode.Combine(X, Y);
}

