using Minioasis.Application.Items.Abstractions;
using Minioasis.Application.Items.Requests;
using Minioasis.Application.Items.Responses;
using Minioasis.Application.Shared.Results;

namespace Minioasis.Api.Tests.Items.TestDoubles;

internal sealed class FakeGetItemUseCase(ApplicationResult<GetItemResponse> result) : IGetItemUseCase
{
    public Task<ApplicationResult<GetItemResponse>> ExecuteAsync(
        GetItemRequest request,
        CancellationToken cancellationToken = default) => Task.FromResult(result);
}
