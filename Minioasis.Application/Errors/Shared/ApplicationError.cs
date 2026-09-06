namespace Minioasis.Application.Errors.Shared;

public sealed record ApplicationError
{
    public ApplicationError(
        ApplicationErrorDefinition definition,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        Definition = definition;
        Metadata = metadata;
    }

    public ApplicationErrorDefinition Definition { get; }

    public string Code => Definition.Code;

    public ApplicationErrorCategory Category => Definition.Category;

    public IReadOnlyDictionary<string, string>? Metadata { get; }
}
