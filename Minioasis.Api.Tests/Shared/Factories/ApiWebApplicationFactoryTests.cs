using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minioasis.Api.Tests.Items.TestDoubles;
using Minioasis.Application.Items.Abstractions;
using Minioasis.Application.Items.Responses;
using Minioasis.Application.Shared.Results;
using Minioasis.Koha.Shared.Options;
using System.Net.Http.Json;
using Xunit;

namespace Minioasis.Api.Tests.Shared.Factories;

public sealed class ApiWebApplicationFactoryTests
{
    [Fact]
    public async Task Hosts_keep_configuration_and_service_overrides_isolated()
    {
        string[] keys = ["MINIOASIS_KOHA_BASE_URL", "MINIOASIS_KOHA_CLIENT_ID", "MINIOASIS_KOHA_CLIENT_SECRET"];
        var environmentBefore = keys.Select(Environment.GetEnvironmentVariable).ToArray();
        var firstItem = new GetItemResponse("12", "34", "FIRST", "QA1");
        var secondItem = new GetItemResponse("12", "56", "SECOND", "QA2");

        await using (var first = CreateFactory("first", firstItem))
        {
            await using (var second = CreateFactory("second", secondItem))
            {
                using var firstClient = first.CreateClient();
                using var secondClient = second.CreateClient();
                var results = await Task.WhenAll(
                    firstClient.GetFromJsonAsync<GetItemResponse>("/api/v1/items/12", TestContext.Current.CancellationToken),
                    secondClient.GetFromJsonAsync<GetItemResponse>("/api/v1/items/12", TestContext.Current.CancellationToken));

                Assert.Equal(firstItem, results[0]);
                Assert.Equal(secondItem, results[1]);
                AssertSettings(first, "first");
                AssertSettings(second, "second");
                Assert.Equal(environmentBefore, keys.Select(Environment.GetEnvironmentVariable).ToArray());
            }
        }

        Assert.Equal(environmentBefore, keys.Select(Environment.GetEnvironmentVariable).ToArray());
    }

    private static ApiWebApplicationFactory CreateFactory(string name, GetItemResponse item) =>
        new(services =>
        {
            services.RemoveAll<IGetItemUseCase>();
            services.AddSingleton<IGetItemUseCase>(new FakeGetItemUseCase(ApplicationResult<GetItemResponse>.Success(item)));
        }, new Dictionary<string, string?>
        {
            ["MINIOASIS_KOHA_BASE_URL"] = $"https://{name}.test/api/v1",
            ["MINIOASIS_KOHA_CLIENT_ID"] = name,
            ["MINIOASIS_KOHA_CLIENT_SECRET"] = $"{name}-secret"
        });

    private static void AssertSettings(ApiWebApplicationFactory factory, string name)
    {
        var options = factory.Services.GetRequiredService<KohaOptions>();
        Assert.Equal(new Uri($"https://{name}.test/api/v1/"), options.BaseUrl);
        Assert.Equal(name, options.ClientId);
        Assert.Equal($"{name}-secret", options.ClientSecret);
        Assert.Equal(name, factory.Services.GetRequiredService<IConfiguration>()["MINIOASIS_KOHA_CLIENT_ID"]);
    }
}
