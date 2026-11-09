using Minioasis.Api.Shared.Abstractions;
using Minioasis.Api.Shared.Factories;
using Minioasis.Api.Shared.Providers;
using Minioasis.Application.Shared.Errors;
using Minioasis.Application.Shared.Results;

namespace Minioasis.Api.Shared.Mappers;

internal sealed class ApplicationResultHttpMapper(
    ErrorMessageProvider messages,
    IHttpContextAccessor context,
    ILogger<ApplicationResultHttpMapper> logger) : IApplicationResultHttpMapper
{
    public IResult Map<T>(ApplicationResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var error = result.Error!;
        var status = error.Category switch
        {
            ApplicationErrorCategory.Validation => 400,
            ApplicationErrorCategory.NotFound => 404,
            ApplicationErrorCategory.DependencyInvalidResponse => 502,
            ApplicationErrorCategory.DependencyUnavailable => 503,
            _ => 500
        };

        var traceId = context.HttpContext?.TraceIdentifier;
        if (status >= 500)
        {
            logger.LogError("Application failure {ErrorCode}. TraceId: {TraceId}", error.Code, traceId);
            return ApiErrorResultFactory.InternalError(status, traceId);
        }

        var text = messages.Get(error.Code);
        var detail = error.Metadata?.Aggregate(
            text.Detail,
            (current, value) => current.Replace(
                "{" + value.Key + "}",
                value.Value,
                StringComparison.Ordinal)) ?? text.Detail;

        return ApiErrorResultFactory.Create(status, text.Title, detail, error.Code, traceId);
    }
}
