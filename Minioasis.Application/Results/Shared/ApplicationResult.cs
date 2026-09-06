using Minioasis.Application.Errors.Shared;

namespace Minioasis.Application.Results.Shared;

public sealed class ApplicationResult<T>
{
    private ApplicationResult(T? value, ApplicationError? error)
    {
        Value = value;
        Error = error;
    }

    public bool IsSuccess => Error is null;

    public T? Value
    {
        get;
    }

    public ApplicationError? Error
    {
        get;
    }

    public static ApplicationResult<T> Success(T value) => new(value, null);

    public static ApplicationResult<T> Failure(ApplicationError error) => new(default, error);
}
