using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PiecePosition
{
    public int X { get; }
    public int Y { get; }


    public PiecePosition(int x, int y)
    {
        X = x;
        Y = y;
    }


    // 靜態方法
    public static bool TryCreate(int x, int y, BoardDimensions board, out PiecePosition position)
    {
        position = null;

        if (x < 0 || x >= board.BoardSizeX || y < 0 || y >= board.BoardSizeY)
            return false;

        position = new PiecePosition(x, y);
        return true;
    }

    // 實例方法
    public static PiecePosition Create(int x, int y, BoardDimensions board)
    {
        if (x < 0 || x >= board.BoardSizeX || y < 0 || y >= board.BoardSizeY)
            throw new ArgumentException($"位置 ({x}, {y}) 超出範圍");

        return new PiecePosition(x, y);
    }


    // 靜態方法：在創建對象「之前」驗證
    public static bool IsValidPosition(int x, int y, BoardDimensions board)
    {
        return x >= 0 && x < board.BoardSizeX && y >= 0 && y < board.BoardSizeY;
    }

    // 實例方法：在創建對象「之後」驗證
    public bool IsValid(BoardDimensions board)
    {
        return X >= 0 && X < board.BoardSizeX && Y >= 0 && Y < board.BoardSizeY;
    }


    public override string ToString() => $"({X}, {Y})";

    // 值對象的相等性比較
    public bool Equals(PiecePosition other)
    {
        if (other == null) return false;
        return X == other.X && Y == other.Y;
    }
    public override bool Equals(object obj) => obj is PiecePosition other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);


    // 操作符重載（== 和 !=）
    public static bool operator ==(PiecePosition left, PiecePosition right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }
    public static bool operator !=(PiecePosition left, PiecePosition right) => !(left == right);
}
