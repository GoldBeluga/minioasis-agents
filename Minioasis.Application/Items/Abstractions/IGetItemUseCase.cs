using Minioasis.Application.Items.Requests;
using Minioasis.Application.Items.Responses;
using Minioasis.Application.Shared.Results;

namespace Minioasis.Application.Items.Abstractions;

public interface IGetItemUseCase
{
    Task<ApplicationResult<GetItemResponse>> ExecuteAsync(
        GetItemRequest request,
        CancellationToken cancellationToken = default);
}
