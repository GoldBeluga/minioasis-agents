using Minioasis.Application.Abstractions.Item;
using Minioasis.Application.Models.Item;
using Minioasis.Application.Results.Item;
using Minioasis.Koha.Configuration.Shared;
using Minioasis.Koha.Exceptions.Authentication;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Minioasis.Koha.Generated.Items;
using Minioasis.Koha.Generated.Items.Contracts;

namespace Minioasis.Koha.Adapters.Item;

internal sealed class KohaItemCatalogAdapter(
    IHttpClientFactory httpClientFactory,
    KohaOptions options,
    ILogger<KohaItemCatalogAdapter> logger) : IItemCatalogGateway
{
    public async Task<ItemLookupResult> FindByIdAsync(
        ExternalItemId itemId,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(itemId.Value, out var kohaId) || kohaId <= 0)
        {
            return ItemLookupResult.InvalidIdentifier();
        }

        try
        {
            var generated = new GeneratedKohaItemClient(
                options.BaseUrl.ToString().TrimEnd('/'),
                httpClientFactory.CreateClient(KohaOptions.ApiClientName));
            var item = await generated.GetItemAsync(kohaId, cancellationToken);

            if (item is null || item.Item_id <= 0 || item.Biblio_id <= 0)
            {
                return ItemLookupResult.InvalidResponse();
            }

            return ItemLookupResult.Found(new ItemDetails(
                item.Item_id.ToString(),
                item.Biblio_id.ToString(),
                item.External_id,
                item.Callnumber));
        }
        catch (KohaServiceException exception) when (exception.StatusCode == 404)
        {
            return ItemLookupResult.NotFound();
        }
        catch (KohaServiceException exception) when (exception.StatusCode is 401 or 403)
        {
            logger.LogError("Catalog authentication failed with status {StatusCode}", exception.StatusCode);
            return ItemLookupResult.AuthenticationFailed();
        }
        catch (KohaServiceException exception) when (exception.StatusCode == 429 || exception.StatusCode >= 500)
        {
            logger.LogWarning("Catalog request failed with status {StatusCode}", exception.StatusCode);
            return ItemLookupResult.Unavailable();
        }
        catch (KohaServiceException exception)
        {
            logger.LogWarning("Catalog returned an unusable response with status {StatusCode}", exception.StatusCode);
            return ItemLookupResult.InvalidResponse();
        }
        catch (KohaAuthenticationException)
        {
            logger.LogError("Catalog OAuth authentication failed");
            return ItemLookupResult.AuthenticationFailed();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Catalog request timed out");
            return ItemLookupResult.Unavailable();
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Catalog connection failed");
            return ItemLookupResult.Unavailable();
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Catalog response could not be read");
            return ItemLookupResult.InvalidResponse();
        }
    }
}
