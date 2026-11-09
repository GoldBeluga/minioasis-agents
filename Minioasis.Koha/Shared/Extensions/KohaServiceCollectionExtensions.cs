using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minioasis.Application.Items.Abstractions;
using Minioasis.Koha.Authentication.Abstractions;
using Minioasis.Koha.Authentication.Handlers;
using Minioasis.Koha.Authentication.Providers;
using Minioasis.Koha.Items.Adapters;
using Minioasis.Koha.Shared.Handlers;
using Minioasis.Koha.Shared.Options;

namespace Minioasis.Koha.Shared.Extensions;

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

        services.AddScoped<IGetItemGateway, KohaGetItemAdapter>();
        return services;
    }
}
