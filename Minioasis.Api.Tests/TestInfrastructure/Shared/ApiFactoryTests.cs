using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minioasis.Api.Tests.TestDoubles.Item;
using Minioasis.Application.Abstractions.Item;
using Minioasis.Application.Models.Item;
using Minioasis.Application.Results.Shared;
using Minioasis.Koha.Configuration.Shared;
using Xunit;

namespace Minioasis.Api.Tests.TestInfrastructure.Shared;

public sealed class ApiFactoryTests
{
    [Fact]
    public async Task Hosts_keep_configuration_and_service_overrides_isolated()
    {
        string[] keys = ["MINIOASIS_KOHA_BASE_URL", "MINIOASIS_KOHA_CLIENT_ID", "MINIOASIS_KOHA_CLIENT_SECRET"];
        var environmentBefore = keys.Select(Environment.GetEnvironmentVariable).ToArray();
        var firstItem = new ItemDetails("12", "34", "FIRST", "QA1");
        var secondItem = new ItemDetails("12", "56", "SECOND", "QA2");

        await using (var first = CreateFactory("first", firstItem))
        await using (var second = CreateFactory("second", secondItem))
        {
            using var firstClient = first.CreateClient();
            using var secondClient = second.CreateClient();
            var results = await Task.WhenAll(
                firstClient.GetFromJsonAsync<ItemDetails>("/api/v1/items/12", TestContext.Current.CancellationToken),
                secondClient.GetFromJsonAsync<ItemDetails>("/api/v1/items/12", TestContext.Current.CancellationToken));

            Assert.Equal(firstItem, results[0]);
            Assert.Equal(secondItem, results[1]);
            AssertSettings(first, "first");
            AssertSettings(second, "second");
            Assert.Equal(environmentBefore, keys.Select(Environment.GetEnvironmentVariable).ToArray());
        }

        Assert.Equal(environmentBefore, keys.Select(Environment.GetEnvironmentVariable).ToArray());
    }

    private static ApiFactory CreateFactory(string name, ItemDetails item) =>
        new(services =>
        {
            services.RemoveAll<IGetItemUseCase>();
            services.AddSingleton<IGetItemUseCase>(new FakeGetItemUseCase(ApplicationResult<ItemDetails>.Success(item)));
        }, new Dictionary<string, string?>
        {
            ["MINIOASIS_KOHA_BASE_URL"] = $"https://{name}.test/api/v1",
            ["MINIOASIS_KOHA_CLIENT_ID"] = name,
            ["MINIOASIS_KOHA_CLIENT_SECRET"] = $"{name}-secret"
        });

    private static void AssertSettings(ApiFactory factory, string name)
    {
        var options = factory.Services.GetRequiredService<KohaOptions>();
        Assert.Equal(new Uri($"https://{name}.test/api/v1/"), options.BaseUrl);
        Assert.Equal(name, options.ClientId);
        Assert.Equal($"{name}-secret", options.ClientSecret);
        Assert.Equal(name, factory.Services.GetRequiredService<IConfiguration>()["MINIOASIS_KOHA_CLIENT_ID"]);
    }
}
