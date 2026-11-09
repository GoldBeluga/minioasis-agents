using Minioasis.Api.Shared.Responses;

namespace Minioasis.Api.Shared.Factories;

internal static class ApiErrorResultFactory
{
    public static IResult InternalError(int status, string? traceId) =>
        Create(status, "Internal error", null, "internal_error", traceId);

    public static IResult Create(int status, string title, string? detail, string code, string? traceId) =>
        Results.Json(
            new ApiErrorResponse(status, title, code, traceId, detail),
            statusCode: status,
            contentType: "application/problem+json");
}
