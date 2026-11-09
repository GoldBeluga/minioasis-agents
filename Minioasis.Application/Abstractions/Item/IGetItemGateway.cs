using Minioasis.Application.Models.Item;
using Minioasis.Application.Results.Item;

namespace Minioasis.Application.Abstractions.Item;

public interface IGetItemGateway
{
    Task<ItemLookupResult> FindByIdAsync(
        ItemId itemId,
        CancellationToken cancellationToken = default);
}
