using Minioasis.Application.Errors.Shared;

namespace Minioasis.Application.Errors.Catalog;

[ErrorDefinitions]
public static class CatalogErrors
{
    public static readonly ApplicationErrorDefinition Unavailable =
        new("catalog.unavailable", ApplicationErrorCategory.DependencyUnavailable);

    public static readonly ApplicationErrorDefinition AuthenticationFailed =
        new("catalog.authentication_failed", ApplicationErrorCategory.DependencyAuthentication);

    public static readonly ApplicationErrorDefinition InvalidResponse =
        new("catalog.invalid_response", ApplicationErrorCategory.DependencyInvalidResponse);
}
