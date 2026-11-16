using Microsoft.Extensions.DependencyInjection;
using Minioasis.Api.Tests.Shared.Factories;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Minioasis.Api.Tests.Items.Endpoints;

// Runs the production composition from Program.cs and replaces only the network below the Koha HTTP clients.
public sealed class ItemEndpointsCompositionTests
{
    [Fact]
    public async Task Route_identifier_reaches_koha_through_the_production_composition()
    {
        var koha = new FakeKohaHandler();
        await using var factory = CreateFactory(koha);

        using var response = await factory.CreateClient().GetAsync(
            "/api/v1/items/%2077%20",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal("77", json.GetProperty("id").GetString());
        Assert.Equal("34", json.GetProperty("bibliographicRecordId").GetString());
        Assert.Equal("BC-77", json.GetProperty("barcode").GetString());
        Assert.Equal("QA77", json.GetProperty("callNumber").GetString());
        Assert.Equal(["/api/v1/oauth/token", "/api/v1/items/77"], koha.Paths);
        Assert.Equal("Bearer token-1", koha.ItemAuthorization);
    }

    [Fact]
    public async Task Invalid_route_identifier_is_rejected_without_calling_koha()
    {
        var koha = new FakeKohaHandler();
        await using var factory = CreateFactory(koha);

        using var response = await factory.CreateClient().GetAsync(
            "/api/v1/items/abc",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(
            TestContext.Current.CancellationToken);
        Assert.Equal("items.invalid_identifier", problem!["code"].ToString());
        Assert.Empty(koha.Paths);
    }

    [Fact]
    public async Task Missing_item_returns_not_found_naming_the_requested_identifier()
    {
        var koha = new FakeKohaHandler();
        await using var factory = CreateFactory(koha);

        using var response = await factory.CreateClient().GetAsync(
            $"/api/v1/items/{FakeKohaHandler.MissingItemId}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(
            TestContext.Current.CancellationToken);
        Assert.Equal("items.not_found", problem!["code"].ToString());
        Assert.Equal(
            $"No item was found with identifier {FakeKohaHandler.MissingItemId}.",
            problem["detail"].ToString());
    }

    [Fact]
    public async Task Item_without_barcode_or_call_number_returns_explicit_nulls()
    {
        var koha = new FakeKohaHandler();
        await using var factory = CreateFactory(koha);

        using var response = await factory.CreateClient().GetAsync(
            $"/api/v1/items/{FakeKohaHandler.BareItemId}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(FakeKohaHandler.BareItemId, json.GetProperty("id").GetString());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("barcode").ValueKind);
        Assert.Equal(JsonValueKind.Null, json.GetProperty("callNumber").ValueKind);
    }

    private static ApiWebApplicationFactory CreateFactory(FakeKohaHandler koha) =>
        new(services => services.ConfigureHttpClientDefaults(builder =>
            builder.ConfigurePrimaryHttpMessageHandler(() => koha)));

    private sealed class FakeKohaHandler : HttpMessageHandler
    {
        public const string MissingItemId = "9001";
        public const string BareItemId = "55";
        private const string ItemPathPrefix = "/api/v1/items/";
        private readonly List<string> paths = [];

        public IReadOnlyList<string> Paths
        {
            get
            {
                lock (paths)
                {
                    return paths.ToArray();
                }
            }
        }

        public string? ItemAuthorization
        {
            get; private set;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            lock (paths)
            {
                paths.Add(path);
            }

            if (path == "/api/v1/oauth/token")
            {
                return Json("""{"access_token":"token-1","token_type":"Bearer","expires_in":3600}""");
            }

            if (path.StartsWith(ItemPathPrefix, StringComparison.Ordinal))
            {
                ItemAuthorization = request.Headers.Authorization?.ToString();
                var id = path[ItemPathPrefix.Length..];
                if (id == MissingItemId)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("{}", Encoding.UTF8, "application/json")
                    });
                }

                if (id == BareItemId)
                {
                    return Json($$"""{"item_id":{{id}},"biblio_id":34}""");
                }

                return Json($$"""{"item_id":{{id}},"biblio_id":34,"external_id":"BC-{{id}}","callnumber":"QA{{id}}"}""");
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        private static Task<HttpResponseMessage> Json(string json) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
    }
}
