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
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("2147483648")]
    public async Task Rejects_non_positive_or_out_of_range_identifier_without_network(string identifier)
    {
        var handler = new StubHttpMessageHandler((_, _) => throw new InvalidOperationException());
        var result = await Adapter(handler).GetItemAsync(
            new ItemId(identifier),
            TestContext.Current.CancellationToken);

        Assert.Equal(GetItemOutcome.InvalidIdentifier, result.Outcome);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task Maps_missing_barcode_and_call_number_to_null()
    {
        var handler = new StubHttpMessageHandler((_, _) => HttpResponseMessageFactory.CreateJsonAsync(
            HttpStatusCode.OK,
            "{\"item_id\":12,\"biblio_id\":34,\"external_id\":null}"));
        var result = await Adapter(handler).GetItemAsync(
            new ItemId("12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(GetItemOutcome.Found, result.Outcome);
        Assert.Equal(new GetItemResponse("12", "34", null, null), result.Response);
    }

    [Theory]
    [InlineData("{\"item_id\":0,\"biblio_id\":34}")]
    [InlineData("{\"item_id\":-1,\"biblio_id\":34}")]
    [InlineData("{\"item_id\":12,\"biblio_id\":0}")]
    [InlineData("{\"item_id\":12,\"biblio_id\":-3}")]
    public async Task Non_positive_identifiers_in_koha_item_are_an_invalid_response(string json)
    {
        var handler = new StubHttpMessageHandler((_, _) => HttpResponseMessageFactory.CreateJsonAsync(
            HttpStatusCode.OK,
            json));
        var result = await Adapter(handler).GetItemAsync(
            new ItemId("12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(GetItemOutcome.InvalidResponse, result.Outcome);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, GetItemOutcome.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized, GetItemOutcome.AuthenticationFailed)]
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
