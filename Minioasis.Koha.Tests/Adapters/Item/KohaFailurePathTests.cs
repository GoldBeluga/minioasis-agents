using Microsoft.Extensions.Logging.Abstractions;
using Minioasis.Application.Abstractions.Item;
using Minioasis.Application.Models.Item;
using Minioasis.Application.Results.Item;
using Minioasis.Koha.Abstractions.Authentication;
using Minioasis.Koha.Adapters.Item;
using Minioasis.Koha.Configuration.Shared;
using Minioasis.Koha.Handlers.Authentication;
using Minioasis.Koha.Services.Authentication;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using Xunit;

namespace Minioasis.Koha.Tests.Adapters.Item;

public sealed class KohaFailurePathTests
{
    [Fact]
    public async Task OAuth_timeout_is_an_authentication_failure()
    {
        using var harness = new Harness(
            (_, cancellationToken) => WaitForever(cancellationToken),
            (_, _) => throw new InvalidOperationException("Item request should not be sent."));

        var result = await harness.Gateway.FindByIdAsync(
            new ExternalItemId("12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(ItemLookupOutcome.AuthenticationFailed, result.Outcome);
    }

    [Fact]
    public async Task Caller_cancellation_during_oauth_is_propagated()
    {
        using var harness = new Harness(
            (_, cancellationToken) => WaitForever(cancellationToken),
            (_, _) => throw new InvalidOperationException("Item request should not be sent."),
            TimeSpan.FromSeconds(5));
        using var cancellation = new CancellationTokenSource();
        var pending = harness.Gateway.FindByIdAsync(new ExternalItemId("12"), cancellation.Token);

        cancellation.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }

    [Fact]
    public async Task Total_deadline_includes_oauth_and_item_response_body()
    {
        using var harness = new Harness(
            async (_, cancellationToken) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);
                return Token();
            },
            (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new StalledStream())
            }),
            TimeSpan.FromMilliseconds(500));
        var elapsed = Stopwatch.StartNew();

        var result = await harness.Gateway.FindByIdAsync(
            new ExternalItemId("12"),
            TestContext.Current.CancellationToken);

        elapsed.Stop();
        Assert.Equal(ItemLookupOutcome.Unavailable, result.Outcome);
        Assert.True(elapsed.Elapsed < TimeSpan.FromMilliseconds(700), $"Elapsed: {elapsed.Elapsed}");
    }

    [Fact]
    public async Task Item_response_disconnection_is_unavailable()
    {
        using var harness = new Harness(
            (_, _) => Task.FromResult(Token()),
            (_, _) => Task.FromResult(Response(new StreamContent(new BrokenStream()))));

        var result = await harness.Gateway.FindByIdAsync(
            new ExternalItemId("12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(ItemLookupOutcome.Unavailable, result.Outcome);
    }

    [Fact]
    public async Task OAuth_response_disconnection_is_an_authentication_failure()
    {
        using var harness = new Harness(
            (_, _) => Task.FromResult(Response(new StreamContent(new BrokenStream()))),
            (_, _) => throw new InvalidOperationException("Item request should not be sent."));

        var result = await harness.Gateway.FindByIdAsync(
            new ExternalItemId("12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(ItemLookupOutcome.AuthenticationFailed, result.Outcome);
    }

    [Fact]
    public async Task Iso_date_is_parsed_independently_of_current_culture()
    {
        using var harness = Harness.WithItemJson(
            """{"item_id":12,"biblio_id":34,"acquisition_date":"2026-09-06"}""");
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");

            var result = await harness.Gateway.FindByIdAsync(
                new ExternalItemId("12"),
                TestContext.Current.CancellationToken);

            Assert.Equal(ItemLookupOutcome.Found, result.Outcome);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public async Task Invalid_date_is_an_invalid_response()
    {
        using var harness = Harness.WithItemJson(
            """{"item_id":12,"biblio_id":34,"acquisition_date":"not-a-date"}""");

        var result = await harness.Gateway.FindByIdAsync(
            new ExternalItemId("12"),
            TestContext.Current.CancellationToken);

        Assert.Equal(ItemLookupOutcome.InvalidResponse, result.Outcome);
    }

    private static HttpResponseMessage Token() => Response(new StringContent(
        """{"access_token":"token","token_type":"Bearer","expires_in":3600}""",
        System.Text.Encoding.UTF8,
        "application/json"));

    private static HttpResponseMessage Response(HttpContent content) => new(HttpStatusCode.OK)
    {
        Content = content
    };

    private static async Task<HttpResponseMessage> WaitForever(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        throw new InvalidOperationException("Unreachable");
    }

    private sealed class Harness : IDisposable
    {
        private readonly KohaAccessTokenProvider tokens;
        private readonly HttpClient oauthClient;
        private readonly HttpClient apiClient;

        public Harness(
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> oauth,
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> item,
            TimeSpan? requestTimeout = null)
        {
            var options = new KohaOptions
            {
                BaseUrl = new Uri("https://koha.test/api/v1/"),
                ClientId = "client-id",
                ClientSecret = "client-secret",
                RequestTimeout = requestTimeout ?? TimeSpan.FromMilliseconds(100)
            };
            oauthClient = new HttpClient(new CallbackHandler(oauth))
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            var clients = new RoutedClientFactory(oauthClient);
            tokens = new KohaAccessTokenProvider(
                clients,
                options,
                TimeProvider.System,
                NullLogger<KohaAccessTokenProvider>.Instance);
            var bearer = new KohaBearerHandler(tokens)
            {
                InnerHandler = new CallbackHandler(item)
            };
            apiClient = new HttpClient(new KohaRequestTimeoutHandler(options)
            {
                InnerHandler = bearer
            })
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            clients.ApiClient = apiClient;
            Gateway = new KohaItemCatalogAdapter(
                clients,
                options,
                NullLogger<KohaItemCatalogAdapter>.Instance);
        }

        public IItemCatalogGateway Gateway
        {
            get;
        }

        public static Harness WithItemJson(string json) => new(
            (_, _) => Task.FromResult(Token()),
            (_, _) => Task.FromResult(Response(new StringContent(
                json,
                System.Text.Encoding.UTF8,
                "application/json"))));

        public void Dispose()
        {
            apiClient.Dispose();
            oauthClient.Dispose();
            tokens.Dispose();
        }
    }

    private sealed class RoutedClientFactory(HttpClient oauthClient) : IHttpClientFactory
    {
        public HttpClient? ApiClient
        {
            get; set;
        }

        public HttpClient CreateClient(string name) =>
            name == KohaOptions.OAuthClientName
                ? oauthClient
                : ApiClient ?? throw new InvalidOperationException("The API client has not been configured.");
    }

    private sealed class CallbackHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => callback(request, cancellationToken);
    }

    private class StalledStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private sealed class BrokenStream : StalledStream
    {
        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromException<int>(new HttpIOException(
                HttpRequestError.ResponseEnded,
                "The response ended prematurely."));
    }
}
