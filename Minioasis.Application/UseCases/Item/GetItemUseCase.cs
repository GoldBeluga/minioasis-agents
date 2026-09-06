using Minioasis.Application.Abstractions.Item;
using Minioasis.Application.Errors.Catalog;
using Minioasis.Application.Errors.Item;
using Minioasis.Application.Errors.System;
using Minioasis.Application.Models.Item;
using Minioasis.Application.Results.Item;
using Minioasis.Application.Results.Shared;
using static Minioasis.Application.Results.Shared.ApplicationResults;

namespace Minioasis.Application.UseCases.Item;

internal sealed class GetItemUseCase(IItemCatalogGateway gateway) : IGetItemUseCase
{
    public async Task<ApplicationResult<ItemDetails>> ExecuteAsync(
        GetItemQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Id))
            return Failure<ItemDetails>(ItemErrors.InvalidIdentifier);

        var id = query.Id.Trim();
        var result = await gateway.FindByIdAsync(new ExternalItemId(id), cancellationToken);

        return result.Outcome switch
        {
            ItemLookupOutcome.Found when result.Item is not null =>
                ApplicationResult<ItemDetails>.Success(result.Item),
            ItemLookupOutcome.NotFound =>
                Failure<ItemDetails>(ItemErrors.NotFound, new Dictionary<string, string> { ["id"] = id }),
            ItemLookupOutcome.InvalidIdentifier =>
                Failure<ItemDetails>(ItemErrors.InvalidIdentifier),
            ItemLookupOutcome.Unavailable =>
                Failure<ItemDetails>(CatalogErrors.Unavailable),
            ItemLookupOutcome.AuthenticationFailed =>
                Failure<ItemDetails>(CatalogErrors.AuthenticationFailed),
            ItemLookupOutcome.InvalidResponse =>
                Failure<ItemDetails>(CatalogErrors.InvalidResponse),
            _ => Failure<ItemDetails>(SystemErrors.Unexpected)
        };
    }
}
