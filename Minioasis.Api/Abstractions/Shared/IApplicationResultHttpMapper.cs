using Minioasis.Application.Results.Shared;

namespace Minioasis.Api.Abstractions.Shared;

public interface IApplicationResultHttpMapper
{
    IResult Map<T>(ApplicationResult<T> result);
}
