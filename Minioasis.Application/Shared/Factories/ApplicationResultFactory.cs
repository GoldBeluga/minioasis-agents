using Minioasis.Application.Shared.Errors;
using Minioasis.Application.Shared.Results;

namespace Minioasis.Application.Shared.Factories;

public static class ApplicationResultFactory
{
    public static ApplicationResult<T> CreateFailure<T>(
        ApplicationErrorDefinition definition,
        IReadOnlyDictionary<string, string>? metadata = null) =>
        ApplicationResult<T>.Failure(new ApplicationError(definition, metadata));
}
