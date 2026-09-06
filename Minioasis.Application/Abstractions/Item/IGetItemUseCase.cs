using Minioasis.Application.Models.Item;
using Minioasis.Application.Results.Shared;

namespace Minioasis.Application.Abstractions.Item;

public interface IGetItemUseCase
{
    Task<ApplicationResult<ItemDetails>> ExecuteAsync(
        GetItemQuery query,
        CancellationToken cancellationToken = default);
}
