using Minioasis.Application.Abstractions.Item;
using Minioasis.Koha.Abstractions.Authentication;
using Minioasis.Koha.Adapters.Item;
using Minioasis.Koha.Configuration.Shared;
using Minioasis.Koha.Handlers.Authentication;
using Minioasis.Koha.Services.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Minioasis.Koha.Extensions.Shared;

public static class KohaServiceCollectionExtensions
{
    public static IServiceCollection AddMinioasisKoha(this IServiceCollection services, IConfiguration configuration)
    {
        var options = KohaOptions.FromConfiguration(configuration);

        services.AddSingleton(options);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IKohaAccessTokenProvider, KohaAccessTokenProvider>();
        services.AddTransient<KohaBearerHandler>();

        services.AddHttpClient(
            KohaOptions.OAuthClientName,
            client => client.Timeout = TimeSpan.FromSeconds(10));

        services.AddHttpClient(
                KohaOptions.ApiClientName,
                client => client.Timeout = TimeSpan.FromSeconds(10))
            .AddHttpMessageHandler<KohaBearerHandler>();

        services.AddScoped<IItemCatalogGateway, KohaItemCatalogAdapter>();
        return services;
    }
}
