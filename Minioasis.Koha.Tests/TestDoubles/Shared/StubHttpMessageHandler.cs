using System.Net;

namespace Minioasis.Koha.Tests.TestDoubles.Shared;

internal sealed class StubHttpMessageHandler(
    Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
{
    public int Calls
    {
        get; private set;
    }

    public List<string?> Authorizations { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Calls++;
        Authorizations.Add(request.Headers.Authorization?.ToString());
        return respond(request, cancellationToken);
    }
}
