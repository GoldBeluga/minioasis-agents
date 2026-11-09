namespace Minioasis.Application.Shared.Errors;

[ErrorDefinitions]
public static class SystemErrorDefinitions
{
    public static readonly ApplicationErrorDefinition Unexpected =
        new("system.unexpected", ApplicationErrorCategory.Unexpected);
}
