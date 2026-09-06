namespace Minioasis.Application.Results.Item;

public enum ItemLookupOutcome
{
    Found,
    NotFound,
    InvalidIdentifier,
    Unavailable,
    AuthenticationFailed,
    InvalidResponse
}
