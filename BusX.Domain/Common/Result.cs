namespace BusX.Domain.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    private Result(bool isSuccess, T? value, string? errorMessage = null, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Failure(string errorMessage, Exception? exception = null) =>
        new(false, default, errorMessage, exception);
}