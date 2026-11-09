using System.Net;

namespace Minioasis.Koha.Tests.Shared.TestDoubles;

internal sealed class SingleHttpClientFactory(HttpClient client) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => client;
}
