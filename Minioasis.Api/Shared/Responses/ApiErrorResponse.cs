using System.Text.Json.Serialization;

namespace Minioasis.Api.Shared.Responses;

internal sealed record ApiErrorResponse(
    int Status,
    string Title,
    string Code,
    string? TraceId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Detail);
