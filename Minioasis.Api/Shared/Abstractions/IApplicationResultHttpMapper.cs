using Minioasis.Application.Shared.Results;

namespace Minioasis.Api.Shared.Abstractions;

public interface IApplicationResultHttpMapper
{
    IResult Map<T>(ApplicationResult<T> result);
}
