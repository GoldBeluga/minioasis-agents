using Microsoft.Extensions.Logging;
using Minioasis.Application.Items.Abstractions;
using Minioasis.Application.Items.Identifiers;
using Minioasis.Application.Items.Responses;
using Minioasis.Application.Items.Results;
using Minioasis.Koha.Authentication.Exceptions;
using Minioasis.Koha.Generated.Items;
using Minioasis.Koha.Generated.Items.Contracts;
using Minioasis.Koha.Shared.Options;
using System.Text.Json;

namespace Minioasis.Koha.Items.Adapters;

internal sealed class KohaGetItemAdapter(
    IHttpClientFactory httpClientFactory,
    KohaOptions options,
    ILogger<KohaGetItemAdapter> logger) : IGetItemGateway
{
    public async Task<GetItemGatewayResult> GetItemAsync(
        ItemId itemId,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(itemId.Value, out var kohaId) || kohaId <= 0)
        {
            return GetItemGatewayResult.InvalidIdentifier();
        }

        try
        {
            var generated = new GeneratedKohaItemClient(
                options.BaseUrl.ToString().TrimEnd('/'),
                httpClientFactory.CreateClient(KohaOptions.ApiClientName));
            var item = await generated.GetItemAsync(kohaId, cancellationToken);

            if (item is null || item.Item_id <= 0 || item.Biblio_id <= 0)
            {
                return GetItemGatewayResult.InvalidResponse();
            }

            return GetItemGatewayResult.Found(new GetItemResponse(
                item.Item_id.ToString(),
                item.Biblio_id.ToString(),
                item.External_id,
                item.Callnumber));
        }
        catch (KohaServiceException exception) when (exception.StatusCode == 404)
        {
            return GetItemGatewayResult.NotFound();
        }
        catch (KohaServiceException exception) when (exception.StatusCode is 401 or 403)
        {
            logger.LogError("Koha GetItem authentication failed with status {StatusCode}", exception.StatusCode);
            return GetItemGatewayResult.AuthenticationFailed();
        }
        catch (KohaServiceException exception) when (exception.StatusCode == 429 || exception.StatusCode >= 500)
        {
            logger.LogWarning("Koha GetItem request failed with status {StatusCode}", exception.StatusCode);
            return GetItemGatewayResult.Unavailable();
        }
        catch (KohaServiceException exception)
        {
            logger.LogWarning("Koha GetItem returned an unusable response with status {StatusCode}", exception.StatusCode);
            return GetItemGatewayResult.InvalidResponse();
        }
        catch (KohaAuthenticationException)
        {
            logger.LogError("Koha GetItem OAuth authentication failed");
            return GetItemGatewayResult.AuthenticationFailed();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Koha GetItem request timed out");
            return GetItemGatewayResult.Unavailable();
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Koha GetItem connection failed");
            return GetItemGatewayResult.Unavailable();
        }
        catch (HttpIOException exception)
        {
            logger.LogWarning(exception, "Koha GetItem response ended unexpectedly");
            return GetItemGatewayResult.Unavailable();
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Koha GetItem response could not be read");
            return GetItemGatewayResult.InvalidResponse();
        }
    }
}
