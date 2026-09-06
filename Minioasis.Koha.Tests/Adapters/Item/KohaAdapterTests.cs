using Minioasis.Application.Models.Item;
using Minioasis.Application.Results.Item;
using Minioasis.Koha.Adapters.Item;
using Minioasis.Koha.Configuration.Shared;
using Minioasis.Koha.Exceptions.Authentication;
using Minioasis.Koha.Tests.TestDoubles.Shared;
using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Minioasis.Koha.Tests.Adapters.Item;

public sealed class KohaAdapterTests
{
    [Fact]
    public async Task Maps_generated_dto_to_application_model()
    {
        var handler = new StubHttpMessageHandler((_, _) => TestResponses.Json(
            HttpStatusCode.OK,
            "{\"item_id\":12,\"biblio_id\":34,\"external_id\":\"ABC\",\"callnumber\":\"QA1\"}"));
        var result = await Adapter(handler).FindByIdAsync(
            new ExternalItemId("12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(ItemLookupOutcome.Found, result.Outcome);
        Assert.Equal(new ItemDetails("12", "34", "ABC", "QA1"), result.Item);
    }

    [Fact]
    public async Task Rejects_non_numeric_identifier_without_network()
    {
        var handler = new StubHttpMessageHandler((_, _) => throw new InvalidOperationException());
        var result = await Adapter(handler).FindByIdAsync(
            new ExternalItemId("koha-12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(ItemLookupOutcome.InvalidIdentifier, result.Outcome);
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, ItemLookupOutcome.NotFound)]
    [InlineData(HttpStatusCode.Forbidden, ItemLookupOutcome.AuthenticationFailed)]
    [InlineData(HttpStatusCode.TooManyRequests, ItemLookupOutcome.Unavailable)]
    [InlineData(HttpStatusCode.BadGateway, ItemLookupOutcome.Unavailable)]
    [InlineData(HttpStatusCode.BadRequest, ItemLookupOutcome.InvalidResponse)]
    public async Task Translates_item_http_failures(
        HttpStatusCode status,
        ItemLookupOutcome expected)
    {
        var handler = new StubHttpMessageHandler((_, _) => TestResponses.Json(status, "{}"));
        var result = await Adapter(handler).FindByIdAsync(
            new ExternalItemId("12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(expected, result.Outcome);
    }

    [Fact]
    public async Task Translates_oauth_failure_to_authentication_outcome()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromException<HttpResponseMessage>(
                new KohaAuthenticationException("OAuth failed")));
        var result = await Adapter(handler).FindByIdAsync(
            new ExternalItemId("12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(ItemLookupOutcome.AuthenticationFailed, result.Outcome);
    }

    private static KohaItemCatalogAdapter Adapter(HttpMessageHandler handler) =>
        new(
            new SingleClientFactory(new HttpClient(handler)),
            new KohaOptions
            {
                BaseUrl = new Uri("https://koha.test/api/v1/"),
                ClientId = "id",
                ClientSecret = "secret"
            },
            NullLogger<KohaItemCatalogAdapter>.Instance);
}
