using Minioasis.Api.Shared.Abstractions;
using System.Reflection;

namespace Minioasis.Api.Shared.Extensions;

public static class EndpointModuleExtensions
{
    public static IServiceCollection AddEndpointModules(this IServiceCollection services)
    {
        var contract = typeof(IEndpointModule);
        foreach (var type in Assembly.GetExecutingAssembly()
                     .DefinedTypes
                     .Where(x => !x.IsAbstract
                         && !x.IsInterface
                         && contract.IsAssignableFrom(x))
                     .Select(x => x.AsType()))
        {

            services.AddSingleton(contract, type);
        }

        return services;
    }

    public static IEndpointRouteBuilder MapEndpointModules(this IEndpointRouteBuilder endpoints)
    {
        foreach (var module in endpoints.ServiceProvider.GetRequiredService<IEnumerable<IEndpointModule>>())
        {

            module.MapEndpoints(endpoints);
        }

        return endpoints;
    }
}
