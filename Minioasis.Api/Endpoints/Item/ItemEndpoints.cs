using Minioasis.Api.Abstractions.Shared;
using Minioasis.Api.Endpoints.Shared;
using Minioasis.Application.Abstractions.Item;
using Minioasis.Application.Models.Item;

namespace Minioasis.Api.Endpoints.Item;

internal sealed class ItemEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(ApiRoutes.ItemById, GetItemAsync)
            .WithName("GetItem")
            .Produces<ItemDetails>()
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
        mapper.Map(await useCase.ExecuteAsync(new GetItemQuery(id), cancellationToken));
}
