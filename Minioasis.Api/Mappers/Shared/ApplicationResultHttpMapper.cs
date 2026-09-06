using Minioasis.Api.Abstractions.Shared;
using Minioasis.Api.Services.Shared;
using Minioasis.Application.Errors.Shared;
using Minioasis.Application.Results.Shared;

namespace Minioasis.Api.Mappers.Shared;

internal sealed class ApplicationResultHttpMapper(
    ErrorMessageProvider messages,
    IHttpContextAccessor context) : IApplicationResultHttpMapper
{
    public IResult Map<T>(ApplicationResult<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Value);

        var error = result.Error!;
        var status = error.Category switch
        {
            ApplicationErrorCategory.Validation => 400,
            ApplicationErrorCategory.NotFound => 404,
            ApplicationErrorCategory.DependencyInvalidResponse => 502,
            ApplicationErrorCategory.DependencyUnavailable => 503,
            _ => 500
        };

        var text = messages.Get(error.Code);
        var detail = error.Metadata?.Aggregate(
            text.Detail,
            (current, value) => current.Replace(
                "{" + value.Key + "}",
                value.Value,
                StringComparison.Ordinal)) ?? text.Detail;

        return Results.Problem(
            statusCode: status,
            title: text.Title,
            detail: detail,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = error.Code,
                ["traceId"] = context.HttpContext?.TraceIdentifier
            });
    }
}
