using Microsoft.AspNetCore.Diagnostics;
using Minioasis.Api.Shared.Factories;

namespace Minioasis.Api.Shared.Handlers;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled API failure. TraceId: {TraceId}", httpContext.TraceIdentifier);
        await ApiErrorResultFactory.InternalError(StatusCodes.Status500InternalServerError, httpContext.TraceIdentifier)
            .ExecuteAsync(httpContext);
        return true;
    }
}
