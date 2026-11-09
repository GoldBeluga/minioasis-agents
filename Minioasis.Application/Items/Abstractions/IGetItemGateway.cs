using Minioasis.Application.Items.Identifiers;
using Minioasis.Application.Items.Results;

namespace Minioasis.Application.Items.Abstractions;

public interface IGetItemGateway
{
    Task<GetItemGatewayResult> GetItemAsync(
        ItemId itemId,
        CancellationToken cancellationToken = default);
}
