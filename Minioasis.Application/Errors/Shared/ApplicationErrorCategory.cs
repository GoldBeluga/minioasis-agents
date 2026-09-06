namespace Minioasis.Application.Errors.Shared;

public enum ApplicationErrorCategory
{
    Validation,
    NotFound,
    DependencyUnavailable,
    DependencyAuthentication,
    DependencyInvalidResponse,
    Unexpected
}
