using Microsoft.Extensions.DependencyInjection;
using Minioasis.Application.Items.Abstractions;
using Minioasis.Application.Items.UseCases;

namespace Minioasis.Application.Shared.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddMinioasisApplication(this IServiceCollection services)
    {
        services.AddScoped<IGetItemUseCase, GetItemUseCase>();
        return services;
    }
}
