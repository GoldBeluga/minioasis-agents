using Minioasis.Application.Shared.Errors;

namespace Minioasis.Application.Items.Errors;

[ErrorDefinitions]
public static class GetItemErrorDefinitions
{
    public static readonly ApplicationErrorDefinition InvalidIdentifier =
        new("items.invalid_identifier", ApplicationErrorCategory.Validation);

    public static readonly ApplicationErrorDefinition NotFound =
        new("items.not_found", ApplicationErrorCategory.NotFound);

    public static readonly ApplicationErrorDefinition Unavailable =
        new("items.get.unavailable", ApplicationErrorCategory.DependencyUnavailable);

    public static readonly ApplicationErrorDefinition AuthenticationFailed =
        new("items.get.authentication_failed", ApplicationErrorCategory.DependencyAuthentication);

    public static readonly ApplicationErrorDefinition InvalidResponse =
        new("items.get.invalid_response", ApplicationErrorCategory.DependencyInvalidResponse);
}
