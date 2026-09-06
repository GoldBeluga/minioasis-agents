using Minioasis.Application.Models.Item;

namespace Minioasis.Application.Results.Item;

public sealed record ItemLookupResult
{
    private ItemLookupResult(ItemLookupOutcome outcome, ItemDetails? item)
    {
        Outcome = outcome;
        Item = item;
    }

    public ItemLookupOutcome Outcome
    {
        get;
    }

    public ItemDetails? Item
    {
        get;
    }

    public static ItemLookupResult Found(ItemDetails item) => new(ItemLookupOutcome.Found, item);

    public static ItemLookupResult NotFound() => new(ItemLookupOutcome.NotFound, null);

    public static ItemLookupResult InvalidIdentifier() => new(ItemLookupOutcome.InvalidIdentifier, null);

    public static ItemLookupResult Unavailable() => new(ItemLookupOutcome.Unavailable, null);

    public static ItemLookupResult AuthenticationFailed() =>
        new(ItemLookupOutcome.AuthenticationFailed, null);

    public static ItemLookupResult InvalidResponse() => new(ItemLookupOutcome.InvalidResponse, null);
}
