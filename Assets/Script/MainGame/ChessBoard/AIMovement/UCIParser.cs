using UnityEngine;

public static class UCIParser
{
    public static Result<UCIMove> Parse(string uciMove)
    {
        if (string.IsNullOrEmpty(uciMove) || uciMove.Length < 4)
            return Result<UCIMove>.Failure($"Invalid UCI format: '{uciMove}'");

        int fromX = uciMove[0] - 'a';
        int fromY = uciMove[1] - '1';
        int toX = uciMove[2] - 'a';
        int toY = uciMove[3] - '1';

        var move = new UCIMove(fromX, fromY, toX, toY);
        return Result<UCIMove>.Success(move);
    }
}

public readonly struct UCIMove
{
    public int FromX { get; }
    public int FromY { get; }
    public int ToX { get; }
    public int ToY { get; }
    public UCIMove(int fromX, int fromY, int toX, int toY)
    {
        FromX = fromX;
        FromY = fromY;
        ToX = toX;
        ToY = toY;
    }

    // 是否超出邊界（協議層面的合法性）
    public bool IsWithinBounds(int maxX, int maxY)
    {
        return FromX >= 0 && FromX < maxX && FromY >= 0 && FromY < maxY
            && ToX >= 0 && ToX < maxX && ToY >= 0 && ToY < maxY;
    }
}