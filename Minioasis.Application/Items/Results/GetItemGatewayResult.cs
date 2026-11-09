using Minioasis.Application.Items.Responses;

namespace Minioasis.Application.Items.Results;

public sealed record GetItemGatewayResult
{
    private GetItemGatewayResult(GetItemOutcome outcome, GetItemResponse? response)
    {
        Outcome = outcome;
        Response = response;
    }

    public GetItemOutcome Outcome
    {
        get;
    }

    public GetItemResponse? Response
    {
        get;
    }

    public static GetItemGatewayResult Found(GetItemResponse response) => new(GetItemOutcome.Found, response);

    public static GetItemGatewayResult NotFound() => new(GetItemOutcome.NotFound, null);

    public static GetItemGatewayResult InvalidIdentifier() => new(GetItemOutcome.InvalidIdentifier, null);

    public static GetItemGatewayResult Unavailable() => new(GetItemOutcome.Unavailable, null);

    public static GetItemGatewayResult AuthenticationFailed() =>
        new(GetItemOutcome.AuthenticationFailed, null);

    public static GetItemGatewayResult InvalidResponse() => new(GetItemOutcome.InvalidResponse, null);
}
