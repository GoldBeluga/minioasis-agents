using Minioasis.Koha.Authentication.Abstractions;
using Minioasis.Koha.Authentication.Handlers;
using Minioasis.Koha.Tests.Authentication.TestDoubles;
using Minioasis.Koha.Tests.Shared.TestDoubles;
using System.Net;
using Xunit;

namespace Minioasis.Koha.Tests.Authentication.Handlers;

public sealed class KohaBearerHandlerTests
{
    [Fact]
    public async Task Adds_bearer_token_to_request()
    {
        var inner = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var tokens = new SequenceKohaAccessTokenProvider("token-1");
        using var client = CreateClient(tokens, inner);

        using var response = await client.GetAsync(
            "https://koha.test/api/v1/items/12",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["Bearer token-1"], inner.Authorizations);
        Assert.Equal(1, tokens.GetCalls);
    }

    [Fact]
    public async Task Retries_get_once_with_fresh_token_after_401()
    {
        var responseNumber = 0;
        var inner = new StubHttpMessageHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(++responseNumber == 1
                ? HttpStatusCode.Unauthorized
                : HttpStatusCode.OK)));
        var tokens = new SequenceKohaAccessTokenProvider("token-1", "token-2");
        using var client = CreateClient(tokens, inner);

        using var response = await client.GetAsync(
            "https://koha.test/api/v1/items/12",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, inner.Calls);
        Assert.Equal(["Bearer token-1", "Bearer token-2"], inner.Authorizations);
        Assert.Equal(1, tokens.Invalidations);
        Assert.Equal(2, tokens.GetCalls);
    }

    [Fact]
    public async Task Does_not_retry_a_second_401()
    {
        var inner = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)));
        var tokens = new SequenceKohaAccessTokenProvider("token-1", "token-2", "token-3");
        using var client = CreateClient(tokens, inner);

        using var response = await client.GetAsync(
            "https://koha.test/api/v1/items/12",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(2, inner.Calls);
        Assert.Equal(1, tokens.Invalidations);
        Assert.Equal(2, tokens.GetCalls);
    }

    [Fact]
    public async Task Does_not_retry_non_get_request()
    {
        var inner = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)));
        var tokens = new SequenceKohaAccessTokenProvider("token-1", "token-2");
        using var client = CreateClient(tokens, inner);

        using var response = await client.PostAsync(
            "https://koha.test/api/v1/items",
            new StringContent("{}"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(1, inner.Calls);
        Assert.Equal(0, tokens.Invalidations);
        Assert.Equal(1, tokens.GetCalls);
    }

    private static HttpClient CreateClient(
        IKohaAccessTokenProvider tokens,
        HttpMessageHandler inner) =>
        new(new KohaBearerHandler(tokens) { InnerHandler = inner });
}
