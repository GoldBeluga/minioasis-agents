using Minioasis.Application.Errors.Shared;

namespace Minioasis.Application.Errors.Item;

[ErrorDefinitions]
public static class ItemErrors
{
    public static readonly ApplicationErrorDefinition InvalidIdentifier =
        new("items.invalid_identifier", ApplicationErrorCategory.Validation);

    public static readonly ApplicationErrorDefinition NotFound =
        new("items.not_found", ApplicationErrorCategory.NotFound);
}
