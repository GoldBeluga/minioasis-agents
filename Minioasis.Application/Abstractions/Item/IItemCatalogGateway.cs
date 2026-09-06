using Minioasis.Application.Models.Item;
using Minioasis.Application.Results.Item;

namespace Minioasis.Application.Abstractions.Item;

public interface IItemCatalogGateway
{
    Task<ItemLookupResult> FindByIdAsync(
        ExternalItemId itemId,
        CancellationToken cancellationToken = default);
}
