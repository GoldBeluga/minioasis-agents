using Minioasis.Application.Abstractions.Item;
using Minioasis.Application.UseCases.Item;
using Microsoft.Extensions.DependencyInjection;

namespace Minioasis.Application.Extensions.Shared;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddMinioasisApplication(this IServiceCollection services)
    {
        services.AddScoped<IGetItemUseCase, GetItemUseCase>();
        return services;
    }
}
