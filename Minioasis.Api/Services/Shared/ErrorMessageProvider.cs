using Minioasis.Api.Models.Shared;
using Minioasis.Application.Errors.Shared;
using System.Collections.Frozen;
using System.Text.Json;

namespace Minioasis.Api.Services.Shared;

internal sealed class ErrorMessageProvider
{
    private readonly IReadOnlyDictionary<string, ErrorMessageText> messages;

    private ErrorMessageProvider(Dictionary<string, ErrorMessageText> messages) =>
        this.messages = messages.ToFrozenDictionary(StringComparer.Ordinal);

    public ErrorMessageText Get(string code) =>
        messages.TryGetValue(code, out var text)
            ? text
            : throw new InvalidOperationException($"No API error text for '{code}'.");

    public static ErrorMessageProvider Load(string root, ApplicationErrorDefinitionRegistry registry) =>
        FromJson(File.ReadAllText(Path.Combine(root, "Resources", "errors.en.json")), registry);

    internal static ErrorMessageProvider FromJson(string json, ApplicationErrorDefinitionRegistry registry)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException("The English error resource must be a JSON object.");

            var messages = new Dictionary<string, ErrorMessageText>(StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            foreach (var entry in document.RootElement.EnumerateObject())
            {
                if (!seen.Add(entry.Name))
                    throw new InvalidOperationException($"Duplicate error message '{entry.Name}'.");

                if (!registry.Definitions.ContainsKey(entry.Name))
                    throw new InvalidOperationException($"Unknown error message '{entry.Name}'.");

                ErrorMessageText? text;
                try
                {
                    text = entry.Value.Deserialize<ErrorMessageText>(options);
                }
                catch (JsonException exception)
                {
                    throw new InvalidOperationException($"Invalid error message '{entry.Name}'.", exception);
                }

                if (text is null || string.IsNullOrWhiteSpace(text.Title) || string.IsNullOrWhiteSpace(text.Detail))
                    throw new InvalidOperationException($"No complete error message for '{entry.Name}'.");

                messages.Add(entry.Name, text);
            }

            var missing = registry.Definitions.Keys.Where(code => !messages.ContainsKey(code))
                .Order(StringComparer.Ordinal).ToArray();
            if (missing.Length > 0)
                throw new InvalidOperationException($"Missing error messages: {string.Join(", ", missing)}.");

            return new(messages);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("The English error resource contains malformed JSON.", exception);
        }
    }
}
