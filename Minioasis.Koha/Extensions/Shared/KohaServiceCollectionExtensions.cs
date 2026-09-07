using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minioasis.Application.Abstractions.Item;
using Minioasis.Koha.Abstractions.Authentication;
using Minioasis.Koha.Adapters.Item;
using Minioasis.Koha.Configuration.Shared;
using Minioasis.Koha.Handlers.Authentication;
using Minioasis.Koha.Services.Authentication;

namespace Minioasis.Koha.Extensions.Shared;

public static class KohaServiceCollectionExtensions
{
    public static IServiceCollection AddMinioasisKoha(this IServiceCollection services, IConfiguration configuration)
    {
        var options = KohaOptions.FromConfiguration(configuration);

        services.AddSingleton(options);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IKohaAccessTokenProvider, KohaAccessTokenProvider>();
        services.AddTransient<KohaRequestTimeoutHandler>();
        services.AddTransient<KohaBearerHandler>();

        services.AddHttpClient(
            KohaOptions.OAuthClientName,
            client => client.Timeout = TimeSpan.FromSeconds(10));

        services.AddHttpClient(
                KohaOptions.ApiClientName,
                client => client.Timeout = Timeout.InfiniteTimeSpan)
            .AddHttpMessageHandler<KohaRequestTimeoutHandler>()
            .AddHttpMessageHandler<KohaBearerHandler>();

        services.AddScoped<IItemCatalogGateway, KohaItemCatalogAdapter>();
        return services;
    }
}
