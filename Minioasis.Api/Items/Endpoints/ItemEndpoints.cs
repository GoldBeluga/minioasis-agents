using Minioasis.Api.Shared.Abstractions;
using Minioasis.Api.Shared.Endpoints;
using Minioasis.Application.Items.Abstractions;
using Minioasis.Application.Items.Requests;
using Minioasis.Application.Items.Responses;

namespace Minioasis.Api.Items.Endpoints;

internal sealed class ItemEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.GetItem, GetItemAsync)
            .WithName("GetItem")
            .Produces<GetItemResponse>()
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500)
            .ProducesProblem(502)
            .ProducesProblem(503);

    private static async Task<IResult> GetItemAsync(
        string id,
        IGetItemUseCase useCase,
        IApplicationResultHttpMapper mapper,
        CancellationToken cancellationToken) =>
        mapper.Map(await useCase.ExecuteAsync(new GetItemRequest(id), cancellationToken));
}
