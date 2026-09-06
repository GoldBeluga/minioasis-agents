using Minioasis.Koha.Configuration.Shared;
using Minioasis.Koha.Exceptions.Authentication;
using Minioasis.Koha.Services.Authentication;
using Minioasis.Koha.Tests.TestDoubles.Authentication;
using Minioasis.Koha.Tests.TestDoubles.Shared;
using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Minioasis.Koha.Tests.Services.Authentication;

public sealed class KohaAccessTokenProviderTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 3, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Uses_generated_contract_to_post_expected_form_fields()
    {
        Uri? requestUri = null;
        string? mediaType = null;
        string? body = null;
        var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            requestUri = request.RequestUri;
            mediaType = request.Content?.Headers.ContentType?.MediaType;
            body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return await TestResponses.Json(HttpStatusCode.OK, ValidToken());
        });

        using var provider = Create(handler);
        var token = await provider.GetAsync(TestContext.Current.CancellationToken);

        Assert.Equal("token-1", token);
        Assert.Equal(new Uri("https://koha.test/api/v1/oauth/token"), requestUri);
        Assert.Equal("application/x-www-form-urlencoded", mediaType);
        var form = ParseForm(body!);
        Assert.Equal("client_credentials", form["grant_type"]);
        Assert.Equal("client-id", form["client_id"]);
        Assert.Equal("client-secret", form["client_secret"]);
    }

    [Fact]
    public async Task Accepts_bearer_token_type_case_insensitively()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            TestResponses.Json(HttpStatusCode.OK, ValidToken(tokenType: "bearer")));
        using var provider = Create(handler);

        Assert.Equal("token-1", await provider.GetAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("{\"access_token\":\"\",\"token_type\":\"Bearer\",\"expires_in\":3600}")]
    [InlineData("{\"access_token\":\"token\",\"expires_in\":3600}")]
    [InlineData("{\"access_token\":\"token\",\"token_type\":\"MAC\",\"expires_in\":3600}")]
    [InlineData("{\"access_token\":\"token\",\"token_type\":\"Bearer\",\"expires_in\":0}")]
    public async Task Rejects_invalid_contract_response(string json)
    {
        var handler = new StubHttpMessageHandler((_, _) => TestResponses.Json(HttpStatusCode.OK, json));
        using var provider = Create(handler);

        await Assert.ThrowsAsync<KohaAuthenticationException>(() =>
            provider.GetAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Reuses_token_while_outside_refresh_margin()
    {
        var handler = new StubHttpMessageHandler((_, _) => TestResponses.Json(HttpStatusCode.OK, ValidToken()));
        var clock = new FakeTimeProvider(Start);
        using var provider = Create(handler, clock);

        await provider.GetAsync(TestContext.Current.CancellationToken);
        clock.Advance(TimeSpan.FromMinutes(30));
        await provider.GetAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task Refreshes_token_at_thirty_second_margin()
    {
        var responseNumber = 0;
        var handler = new StubHttpMessageHandler((_, _) =>
            TestResponses.Json(HttpStatusCode.OK, ValidToken($"token-{++responseNumber}", expiresIn: 60)));
        var clock = new FakeTimeProvider(Start);
        using var provider = Create(handler, clock);

        Assert.Equal("token-1", await provider.GetAsync(TestContext.Current.CancellationToken));
        clock.Advance(TimeSpan.FromSeconds(29));
        Assert.Equal("token-1", await provider.GetAsync(TestContext.Current.CancellationToken));
        clock.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal("token-2", await provider.GetAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task Invalidate_forces_refresh()
    {
        var responseNumber = 0;
        var handler = new StubHttpMessageHandler((_, _) =>
            TestResponses.Json(HttpStatusCode.OK, ValidToken($"token-{++responseNumber}")));
        using var provider = Create(handler);

        Assert.Equal("token-1", await provider.GetAsync(TestContext.Current.CancellationToken));
        provider.Invalidate();
        Assert.Equal("token-2", await provider.GetAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task Concurrent_callers_share_one_refresh()
    {
        var handler = new StubHttpMessageHandler((_, _) => TestResponses.Json(HttpStatusCode.OK, ValidToken()));
        using var provider = Create(handler);

        var calls = Enumerable.Range(0, 20)
            .Select(_ => provider.GetAsync(TestContext.Current.CancellationToken));
        var tokens = await Task.WhenAll(calls);

        Assert.All(tokens, token => Assert.Equal("token-1", token));
        Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData((HttpStatusCode)418)]
    public async Task Converts_all_oauth_http_failures_to_authentication_failure(HttpStatusCode status)
    {
        var handler = new StubHttpMessageHandler((_, _) => TestResponses.Json(status, "{}"));
        using var provider = Create(handler);

        await Assert.ThrowsAsync<KohaAuthenticationException>(() =>
            provider.GetAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Converts_malformed_json_to_authentication_failure()
    {
        var handler = new StubHttpMessageHandler((_, _) => TestResponses.Json(HttpStatusCode.OK, "{"));
        using var provider = Create(handler);

        await Assert.ThrowsAsync<KohaAuthenticationException>(() =>
            provider.GetAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Converts_network_failure_to_authentication_failure()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("offline")));
        using var provider = Create(handler);

        await Assert.ThrowsAsync<KohaAuthenticationException>(() =>
            provider.GetAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Converts_timeout_to_authentication_failure()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromException<HttpResponseMessage>(new TaskCanceledException("timeout")));
        using var provider = Create(handler);

        await Assert.ThrowsAsync<KohaAuthenticationException>(() =>
            provider.GetAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Preserves_caller_cancellation()
    {
        var handler = new StubHttpMessageHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Unreachable");
        });
        using var provider = Create(handler);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => provider.GetAsync(cancellation.Token));
    }

    private static KohaAccessTokenProvider Create(
        HttpMessageHandler handler,
        TimeProvider? clock = null) =>
        new(
            new SingleClientFactory(new HttpClient(handler)),
            Options(),
            clock ?? new FakeTimeProvider(Start),
            NullLogger<KohaAccessTokenProvider>.Instance);

    private static KohaOptions Options() => new()
    {
        BaseUrl = new Uri("https://koha.test/api/v1/"),
        ClientId = "client-id",
        ClientSecret = "client-secret"
    };

    private static string ValidToken(
        string accessToken = "token-1",
        string tokenType = "Bearer",
        int expiresIn = 3600) =>
        $$"""{"access_token":"{{accessToken}}","token_type":"{{tokenType}}","expires_in":{{expiresIn}}}""";

    private static Dictionary<string, string> ParseForm(string body) =>
        body.Split('&')
            .Select(part => part.Split('=', 2))
            .ToDictionary(
                pair => Uri.UnescapeDataString(pair[0].Replace('+', ' ')),
                pair => Uri.UnescapeDataString(pair[1].Replace('+', ' ')));
}
