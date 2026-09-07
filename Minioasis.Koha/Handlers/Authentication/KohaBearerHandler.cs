using Minioasis.Koha.Abstractions.Authentication;
using Minioasis.Koha.Exceptions.Authentication;
using System.Net;
using System.Net.Http.Headers;

namespace Minioasis.Koha.Handlers.Authentication;

internal sealed class KohaBearerHandler(IKohaAccessTokenProvider tokens) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await GetTokenAsync(cancellationToken));

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized || request.Method != HttpMethod.Get)
            return response;

        response.Dispose();
        tokens.Invalidate();

        using var retry = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (var header in request.Headers)
            retry.Headers.TryAddWithoutValidation(header.Key, header.Value);

        retry.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await GetTokenAsync(cancellationToken));

        return await base.SendAsync(retry, cancellationToken);
    }

    private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await tokens.GetAsync(cancellationToken);
        }
        catch (OperationCanceledException exception)
        {
            throw new KohaTokenAcquisitionCanceledException(exception);
        }
    }
}
