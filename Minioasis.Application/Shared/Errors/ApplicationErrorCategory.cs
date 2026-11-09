namespace Minioasis.Application.Shared.Errors;

public enum ApplicationErrorCategory
{
    Validation,
    NotFound,
    DependencyUnavailable,
    DependencyAuthentication,
    DependencyInvalidResponse,
    Unexpected
}
