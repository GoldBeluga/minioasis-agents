using Minioasis.Api.Shared.Abstractions;
using Minioasis.Api.Shared.Handlers;
using Minioasis.Api.Shared.Mappers;
using Minioasis.Api.Shared.Providers;
using Minioasis.Application.Shared.Errors;

namespace Minioasis.Api.Shared.Extensions;

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
