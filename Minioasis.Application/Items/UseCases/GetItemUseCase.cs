using Minioasis.Application.Items.Abstractions;
using Minioasis.Application.Items.Errors;
using Minioasis.Application.Items.Identifiers;
using Minioasis.Application.Items.Requests;
using Minioasis.Application.Items.Responses;
using Minioasis.Application.Items.Results;
using Minioasis.Application.Shared.Errors;
using Minioasis.Application.Shared.Results;
using static Minioasis.Application.Shared.Factories.ApplicationResultFactory;

namespace Minioasis.Application.Items.UseCases;

internal sealed class GetItemUseCase(IGetItemGateway gateway) : IGetItemUseCase
{
    public async Task<ApplicationResult<GetItemResponse>> ExecuteAsync(
        GetItemRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ItemId))
        {
            return CreateFailure<GetItemResponse>(GetItemErrorDefinitions.InvalidIdentifier);
        }

        var id = request.ItemId.Trim();
        var result = await gateway.GetItemAsync(new ItemId(id), cancellationToken);

        return result.Outcome switch
        {
            GetItemOutcome.Found when result.Response is not null =>
                ApplicationResult<GetItemResponse>.Success(result.Response),
            GetItemOutcome.NotFound =>
                CreateFailure<GetItemResponse>(GetItemErrorDefinitions.NotFound, new Dictionary<string, string> { ["id"] = id }),
            GetItemOutcome.InvalidIdentifier =>
                CreateFailure<GetItemResponse>(GetItemErrorDefinitions.InvalidIdentifier),
            GetItemOutcome.Unavailable =>
                CreateFailure<GetItemResponse>(GetItemErrorDefinitions.Unavailable),
            GetItemOutcome.AuthenticationFailed =>
                CreateFailure<GetItemResponse>(GetItemErrorDefinitions.AuthenticationFailed),
            GetItemOutcome.InvalidResponse =>
                CreateFailure<GetItemResponse>(GetItemErrorDefinitions.InvalidResponse),
            _ => CreateFailure<GetItemResponse>(SystemErrorDefinitions.Unexpected)
        };
    }
}
