using Microsoft.Extensions.Configuration;

namespace Minioasis.Koha.Configuration.Shared;

public sealed class KohaOptions
{
    internal const string ApiClientName = "Minioasis.Koha";
    internal const string OAuthClientName = "Minioasis.Koha.OAuth";

    internal TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(10);

    public required Uri BaseUrl
    {
        get; init;
    }

    public required string ClientId
    {
        get; init;
    }

    public required string ClientSecret
    {
        get; init;
    }

    internal static KohaOptions FromConfiguration(IConfiguration configuration)
    {
        var text = Require(configuration, "MINIOASIS_KOHA_BASE_URL");
        if (!Uri.TryCreate(text.TrimEnd('/') + "/", UriKind.Absolute, out var uri))
            throw new InvalidOperationException("MINIOASIS_KOHA_BASE_URL must be an absolute URL.");

        return new()
        {
            BaseUrl = uri,
            ClientId = Require(configuration, "MINIOASIS_KOHA_CLIENT_ID"),
            ClientSecret = Require(configuration, "MINIOASIS_KOHA_CLIENT_SECRET")
        };
    }

    private static string Require(IConfiguration configuration, string name) =>
        configuration[name] is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Required environment variable {name} is missing.");
}
