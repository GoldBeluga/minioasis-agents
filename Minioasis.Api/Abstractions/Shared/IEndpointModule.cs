namespace Minioasis.Api.Abstractions.Shared;

public interface IEndpointModule
{
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
