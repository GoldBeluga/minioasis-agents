using Minioasis.Koha.Authentication.Exceptions;
using Minioasis.Koha.Shared.Options;

namespace Minioasis.Koha.Shared.Handlers;

internal sealed class KohaRequestTimeoutHandler(KohaOptions options) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(options.RequestTimeout);

        try
        {
            var response = await base.SendAsync(request, timeout.Token);
            try
            {
                if (response.Content is not null)
                {
                    await response.Content.LoadIntoBufferAsync(timeout.Token);
                }

                return response;
            }
            catch
            {
                response.Dispose();
                throw;
            }
        }
        catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("The Koha request was canceled.", exception, cancellationToken);
        }
        catch (KohaTokenAcquisitionCanceledException exception)
        {
            throw new KohaAuthenticationException("Koha OAuth token acquisition failed.", exception);
        }
    }
}
