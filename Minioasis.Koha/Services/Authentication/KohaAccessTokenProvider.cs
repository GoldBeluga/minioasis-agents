using Minioasis.Koha.Abstractions.Authentication;
using Minioasis.Koha.Configuration.Shared;
using Minioasis.Koha.Exceptions.Authentication;
using Microsoft.Extensions.Logging;
using Minioasis.Koha.Generated.OAuth;
using Minioasis.Koha.Generated.OAuth.Contracts;
using OAuthServiceException = Minioasis.Koha.Generated.OAuth.Contracts.KohaServiceException;

namespace Minioasis.Koha.Services.Authentication;

internal sealed class KohaAccessTokenProvider(
    IHttpClientFactory clients,
    KohaOptions options,
    TimeProvider timeProvider,
    ILogger<KohaAccessTokenProvider> logger) : IKohaAccessTokenProvider, IDisposable
{
    private static readonly TimeSpan RefreshSkew = TimeSpan.FromSeconds(30);
    private readonly SemaphoreSlim refreshLock = new(1, 1);
    private Token? cached;

    public async Task<string> GetAsync(CancellationToken cancellationToken)
    {
        if (IsUsable(cached))
        {
            return cached!.AccessToken;
        }

        await refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (IsUsable(cached))
            {
                return cached!.AccessToken;
            }

            try
            {
                var client = new GeneratedKohaOAuthClient(
                    options.BaseUrl.ToString().TrimEnd('/'),
                    clients.CreateClient(KohaOptions.OAuthClientName));

                var response = await client.TokenOAuthAsync(
                    new OauthTokenRequest
                    {
                        Grant_type = "client_credentials",
                        Client_id = options.ClientId,
                        Client_secret = options.ClientSecret
                    },
                    cancellationToken);

                Validate(response);

                cached = new Token(
                    response.Access_token,
                    timeProvider.GetUtcNow().AddSeconds(response.Expires_in));

                return cached.AccessToken;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (KohaAuthenticationException)
            {
                throw;
            }
            catch (OAuthServiceException exception)
            {
                logger.LogError(
                    "Koha OAuth token acquisition failed with status {StatusCode}",
                    exception.StatusCode);
                throw new KohaAuthenticationException("Koha OAuth token acquisition failed.", exception);
            }
            catch (HttpRequestException exception)
            {
                logger.LogError("Koha OAuth endpoint could not be reached");
                throw new KohaAuthenticationException("Koha OAuth token acquisition failed.", exception);
            }
            catch (OperationCanceledException exception)
            {
                logger.LogError("Koha OAuth token acquisition timed out");
                throw new KohaAuthenticationException("Koha OAuth token acquisition failed.", exception);
            }
        }
        finally
        {
            refreshLock.Release();
        }
    }

    public void Invalidate() => cached = null;

    public void Dispose() => refreshLock.Dispose();

    private bool IsUsable(Token? token) =>
        token is not null && token.ExpiresAt - RefreshSkew > timeProvider.GetUtcNow();

    private void Validate(OauthTokenResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Access_token))
        {
            logger.LogError("Koha OAuth response did not contain an access token");
            throw new KohaAuthenticationException("Koha OAuth response was invalid.");
        }

        if (response.Expires_in <= 0)
        {
            logger.LogError("Koha OAuth response contained an invalid expiry");
            throw new KohaAuthenticationException("Koha OAuth response was invalid.");
        }

        if (!string.Equals(response.Token_type, "Bearer", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogError("Koha OAuth response contained an unsupported token type");
            throw new KohaAuthenticationException("Koha OAuth response was invalid.");
        }
    }

    private sealed record Token(string AccessToken, DateTimeOffset ExpiresAt);
}
