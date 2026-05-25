using System;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }

    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error ?? string.Empty;
    }

    // 成功工廠方法
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    // 失敗工廠方法
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default(T), error);
    }
}