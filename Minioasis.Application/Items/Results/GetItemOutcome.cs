namespace Minioasis.Application.Items.Results;

public enum GetItemOutcome
{
    Found,
    NotFound,
    InvalidIdentifier,
    Unavailable,
    AuthenticationFailed,
    InvalidResponse
}
