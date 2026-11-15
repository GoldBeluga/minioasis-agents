using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minioasis.Application.Items.Abstractions;
using Minioasis.Application.Items.Identifiers;
using Minioasis.Application.Items.Results;
using Minioasis.Koha.Shared.Extensions;
using Minioasis.Koha.Shared.Options;
using System.Diagnostics;
using Xunit;

namespace Minioasis.Koha.Tests.Shared.Extensions;

public sealed class KohaServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData("MINIOASIS_KOHA_BASE_URL", null, "Required environment variable MINIOASIS_KOHA_BASE_URL is missing.")]
    [InlineData("MINIOASIS_KOHA_CLIENT_ID", "", "Required environment variable MINIOASIS_KOHA_CLIENT_ID is missing.")]
    [InlineData("MINIOASIS_KOHA_CLIENT_SECRET", null, "Required environment variable MINIOASIS_KOHA_CLIENT_SECRET is missing.")]
    [InlineData("MINIOASIS_KOHA_BASE_URL", "relative/path", "MINIOASIS_KOHA_BASE_URL must be an absolute URL.")]
    public void Invalid_settings_fail_during_registration(string key, string? value, string message)
    {
        var settings = new Dictionary<string, string?>
        {
            ["MINIOASIS_KOHA_BASE_URL"] = "https://koha.test/api/v1/",
            ["MINIOASIS_KOHA_CLIENT_ID"] = "test",
            ["MINIOASIS_KOHA_CLIENT_SECRET"] = "test"
        };
        settings[key] = value;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() => services.AddMinioasisKoha(configuration));

        Assert.Equal(message, exception.Message);
        Assert.Empty(services);
    }

    [Fact]
    public void Registered_koha_deadline_is_ten_seconds()
    {
        using var provider = CreateServices().BuildServiceProvider();

        Assert.Equal(TimeSpan.FromSeconds(10), provider.GetRequiredService<KohaOptions>().RequestTimeout);
    }

    [Fact]
    public async Task Registered_api_client_bounds_token_acquisition_by_the_deadline()
    {
        var services = CreateServices();
        services.AddSingleton(new KohaOptions
        {
            BaseUrl = new Uri("https://koha.test/api/v1/"),
            ClientId = "test",
            ClientSecret = "test",
            RequestTimeout = TimeSpan.FromMilliseconds(300)
        });
        services.ConfigureHttpClientDefaults(builder =>
            builder.ConfigurePrimaryHttpMessageHandler(() => new UnansweredHandler()));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IGetItemGateway>();
        var elapsed = Stopwatch.StartNew();

        var result = await gateway.GetItemAsync(new ItemId("12"), TestContext.Current.CancellationToken);

        elapsed.Stop();
        Assert.Equal(GetItemOutcome.AuthenticationFailed, result.Outcome);
        Assert.True(elapsed.Elapsed < TimeSpan.FromSeconds(5), $"Elapsed: {elapsed.Elapsed}");
    }

    private static ServiceCollection CreateServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MINIOASIS_KOHA_BASE_URL"] = "https://koha.test/api/v1/",
                ["MINIOASIS_KOHA_CLIENT_ID"] = "test",
                ["MINIOASIS_KOHA_CLIENT_SECRET"] = "test"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMinioasisKoha(configuration);
        return services;
    }

    private sealed class UnansweredHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Unreachable");
        }
    }
}
