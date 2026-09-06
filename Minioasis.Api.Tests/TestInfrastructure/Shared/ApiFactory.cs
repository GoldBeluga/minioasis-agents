using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Minioasis.Api.Tests.TestInfrastructure.Shared;

internal sealed class ApiFactory(
    Action<IServiceCollection>? configureServices = null,
    IReadOnlyDictionary<string, string?>? settings = null) : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Host configuration is available before Program registers and validates Koha.
        builder.ConfigureHostConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MINIOASIS_KOHA_BASE_URL"] = "https://koha.test/api/v1/",
                ["MINIOASIS_KOHA_CLIENT_ID"] = "test",
                ["MINIOASIS_KOHA_CLIENT_SECRET"] = "test"
            });
            if (settings is not null)
                configuration.AddInMemoryCollection(settings);
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (configureServices is not null)
            builder.ConfigureTestServices(configureServices);
    }
}
