using System.Collections.Frozen;
using System.Reflection;

namespace Minioasis.Application.Errors.Shared;

public sealed class ApplicationErrorDefinitionRegistry
{
    private ApplicationErrorDefinitionRegistry(Dictionary<string, ApplicationErrorDefinition> definitions) =>
        Definitions = definitions.ToFrozenDictionary(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, ApplicationErrorDefinition> Definitions { get; }

    public static ApplicationErrorDefinitionRegistry Discover() =>
        Discover(typeof(ApplicationErrorDefinitionRegistry).Assembly.DefinedTypes);

    internal static ApplicationErrorDefinitionRegistry Discover(IEnumerable<Type> types)
    {
        var definitions = new Dictionary<string, ApplicationErrorDefinition>(StringComparer.Ordinal);

        foreach (var type in types.Where(type => type.IsDefined(typeof(ErrorDefinitionsAttribute), false)))
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(field => field.IsInitOnly && field.FieldType == typeof(ApplicationErrorDefinition)))
            {
                var source = $"{type.FullName}.{field.Name}";
                if (field.GetValue(null) is not ApplicationErrorDefinition definition)
                    throw new InvalidOperationException($"Error definition '{source}' is null.");

                if (string.IsNullOrWhiteSpace(definition.Code))
                    throw new InvalidOperationException($"Error definition '{source}' has a blank identifier.");

                if (!Enum.IsDefined(definition.Category))
                    throw new InvalidOperationException($"Error definition '{definition.Code}' has an invalid category.");

                if (!definitions.TryAdd(definition.Code, definition))
                    throw new InvalidOperationException($"Duplicate error definition '{definition.Code}'.");
            }
        }

        if (definitions.Count == 0)
            throw new InvalidOperationException("No application error definitions were discovered.");

        return new(definitions);
    }
}
