namespace backend.Services.Errors;

public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new Result(true, null);

    public static Result Fail(Error error) => new Result(false, error);

    public static Result Fail(ErrorCode code) => 
        new Result(false, Error.From(code));
}

public class Result<T>: Result
{
    public T? Data { get; }

    protected Result(bool isSuccess, T? data, Error? error): base(isSuccess, error)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new Result<T>(true, data, null);

    public static new Result<T> Fail(Error error) => new Result<T>(false, default, error);

    public static new Result<T> Fail(ErrorCode code) => 
        new Result<T>(false, default, Error.From(code));
}