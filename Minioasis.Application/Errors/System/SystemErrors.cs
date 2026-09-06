using Minioasis.Application.Errors.Shared;

namespace Minioasis.Application.Errors.System;

[ErrorDefinitions]
public static class SystemErrors
{
    public static readonly ApplicationErrorDefinition Unexpected =
        new("system.unexpected", ApplicationErrorCategory.Unexpected);
}
