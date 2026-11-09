using Microsoft.Extensions.Logging.Abstractions;
using Minioasis.Application.Items.Identifiers;
using Minioasis.Application.Items.Responses;
using Minioasis.Application.Items.Results;
using Minioasis.Koha.Authentication.Exceptions;
using Minioasis.Koha.Items.Adapters;
using Minioasis.Koha.Shared.Options;
using Minioasis.Koha.Tests.Shared.TestDoubles;
using System.Net;
using Xunit;

namespace Minioasis.Koha.Tests.Items.Adapters;

public sealed class KohaGetItemAdapterTests
{
    [Fact]
    public async Task Maps_generated_item_to_get_item_response()
    {
        var handler = new StubHttpMessageHandler((_, _) => HttpResponseMessageFactory.CreateJsonAsync(
            HttpStatusCode.OK,
            "{\"item_id\":12,\"biblio_id\":34,\"external_id\":\"ABC\",\"callnumber\":\"QA1\"}"));
        var result = await Adapter(handler).GetItemAsync(
            new ItemId("12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(GetItemOutcome.Found, result.Outcome);
        Assert.Equal(new GetItemResponse("12", "34", "ABC", "QA1"), result.Response);
    }

    [Fact]
    public async Task Rejects_non_numeric_identifier_without_network()
    {
        var handler = new StubHttpMessageHandler((_, _) => throw new InvalidOperationException());
        var result = await Adapter(handler).GetItemAsync(
            new ItemId("koha-12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(GetItemOutcome.InvalidIdentifier, result.Outcome);
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, GetItemOutcome.NotFound)]
    [InlineData(HttpStatusCode.Forbidden, GetItemOutcome.AuthenticationFailed)]
    [InlineData(HttpStatusCode.TooManyRequests, GetItemOutcome.Unavailable)]
    [InlineData(HttpStatusCode.BadGateway, GetItemOutcome.Unavailable)]
    [InlineData(HttpStatusCode.BadRequest, GetItemOutcome.InvalidResponse)]
    public async Task Translates_item_http_failures(
        HttpStatusCode status,
        GetItemOutcome expected)
    {
        var handler = new StubHttpMessageHandler((_, _) => HttpResponseMessageFactory.CreateJsonAsync(status, "{}"));
        var result = await Adapter(handler).GetItemAsync(
            new ItemId("12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(expected, result.Outcome);
    }

    [Fact]
    public async Task Translates_oauth_failure_to_authentication_outcome()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromException<HttpResponseMessage>(
                new KohaAuthenticationException("OAuth failed")));
        var result = await Adapter(handler).GetItemAsync(
            new ItemId("12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(GetItemOutcome.AuthenticationFailed, result.Outcome);
    }

    private static KohaGetItemAdapter Adapter(HttpMessageHandler handler) =>
        new(
            new SingleHttpClientFactory(new HttpClient(handler)),
            new KohaOptions
            {
                BaseUrl = new Uri("https://koha.test/api/v1/"),
                ClientId = "id",
                ClientSecret = "secret"
            },
            NullLogger<KohaGetItemAdapter>.Instance);
}
