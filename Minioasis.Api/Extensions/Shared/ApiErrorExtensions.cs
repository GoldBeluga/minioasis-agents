using Minioasis.Api.Abstractions.Shared;
using Minioasis.Api.Mappers.Shared;
using Minioasis.Api.Services.Shared;
using Minioasis.Api.Shared.Handlers;
using Minioasis.Application.Errors.Shared;

namespace Minioasis.Api.Extensions.Shared;

internal static class ApiErrorExtensions
{
    public static IServiceCollection AddApiErrorHandling(this IServiceCollection services, string contentRoot)
    {
        services.AddHttpContextAccessor();
        services.AddExceptionHandler<ApiExceptionHandler>();
        var registry = ApplicationErrorDefinitionRegistry.Discover();
        services.AddSingleton(registry);
        services.AddSingleton(ErrorMessageProvider.Load(contentRoot, registry));
        services.AddSingleton<IApplicationResultHttpMapper, ApplicationResultHttpMapper>();
        return services;
    }
}
