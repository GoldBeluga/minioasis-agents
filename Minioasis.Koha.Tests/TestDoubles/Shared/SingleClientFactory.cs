using System.Net;

namespace Minioasis.Koha.Tests.TestDoubles.Shared;

internal sealed class SingleClientFactory(HttpClient client) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => client;
}
