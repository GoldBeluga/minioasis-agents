using Minioasis.Application.Errors.Shared;

namespace Minioasis.Application.Results.Shared;

public static class ApplicationResults
{
    public static ApplicationResult<T> Failure<T>(
        ApplicationErrorDefinition definition,
        IReadOnlyDictionary<string, string>? metadata = null) =>
        ApplicationResult<T>.Failure(new ApplicationError(definition, metadata));
}
