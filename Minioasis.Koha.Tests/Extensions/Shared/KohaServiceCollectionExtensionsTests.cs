using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minioasis.Koha.Extensions.Shared;
using Xunit;

namespace Minioasis.Koha.Tests.Extensions.Shared;

public sealed class KohaServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData("MINIOASIS_KOHA_BASE_URL", null, "Required environment variable MINIOASIS_KOHA_BASE_URL is missing.")]
    [InlineData("MINIOASIS_KOHA_CLIENT_ID", "", "Required environment variable MINIOASIS_KOHA_CLIENT_ID is missing.")]
    [InlineData("MINIOASIS_KOHA_CLIENT_SECRET", null, "Required environment variable MINIOASIS_KOHA_CLIENT_SECRET is missing.")]
    [InlineData("MINIOASIS_KOHA_BASE_URL", "relative/path", "MINIOASIS_KOHA_BASE_URL must be an absolute URL.")]
    public void Invalid_settings_fail_during_registration(string key, string? value, string message)
    {
        var settings = new Dictionary<string, string?>
        {
            ["MINIOASIS_KOHA_BASE_URL"] = "https://koha.test/api/v1/",
            ["MINIOASIS_KOHA_CLIENT_ID"] = "test",
            ["MINIOASIS_KOHA_CLIENT_SECRET"] = "test"
        };
        settings[key] = value;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() => services.AddMinioasisKoha(configuration));

        Assert.Equal(message, exception.Message);
        Assert.Empty(services);
    }
}
