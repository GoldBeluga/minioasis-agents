namespace Minioasis.Api.Shared.Abstractions;

public interface IEndpointModule
{
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
