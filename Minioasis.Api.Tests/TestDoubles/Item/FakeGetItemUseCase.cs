using Minioasis.Application.Abstractions.Item;
using Minioasis.Application.Models.Item;
using Minioasis.Application.Results.Shared;

namespace Minioasis.Api.Tests.TestDoubles.Item;

internal sealed class FakeGetItemUseCase(ApplicationResult<ItemDetails> result) : IGetItemUseCase
{
    public Task<ApplicationResult<ItemDetails>> ExecuteAsync(
        GetItemQuery query,
        CancellationToken cancellationToken = default) => Task.FromResult(result);
}
