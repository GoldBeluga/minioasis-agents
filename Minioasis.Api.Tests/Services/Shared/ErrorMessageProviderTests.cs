using Microsoft.Extensions.DependencyInjection;
using Minioasis.Api.Extensions.Shared;
using Minioasis.Api.Services.Shared;
using Minioasis.Application.Errors.Item;
using Minioasis.Application.Errors.Shared;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace Minioasis.Api.Tests.Services.Shared;

public sealed class ErrorMessageProviderTests
{
    private static readonly ApplicationErrorDefinitionRegistry Registry = ApplicationErrorDefinitionRegistry.Discover();
    private static readonly string[] ClientErrorCodes = [ItemErrors.InvalidIdentifier.Code, ItemErrors.NotFound.Code];

    [Fact]
    public void Loads_client_messages_without_requiring_internal_messages_and_preserves_text()
    {
        var provider = ErrorMessageProvider.FromJson(ValidResource().ToJsonString(), Registry);
        foreach (var code in ClientErrorCodes)
        {
            Assert.Equal("Title", provider.Get(code).Title);
            Assert.Equal("Detail {id}", provider.Get(code).Detail);
        }
        Assert.Throws<InvalidOperationException>(() => provider.Get("unknown"));
        Assert.Throws<InvalidOperationException>(() => provider.Get(ItemErrors.NotFound.Code.ToUpperInvariant()));
    }

    [Theory]
    [InlineData("catalog.unavailable")]
    [InlineData("catalog.authentication_failed")]
    [InlineData("catalog.invalid_response")]
    [InlineData("system.unexpected")]
    public void Rejects_internal_server_error_messages(string code)
    {
        var resource = ValidResource();
        resource[code] = new JsonObject { ["title"] = "Internal error", ["detail"] = "Internal error" };
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ErrorMessageProvider.FromJson(resource.ToJsonString(), Registry));
        Assert.Contains(code, exception.Message);
        Assert.Contains("must not have an API error message", exception.Message);
    }

    [Fact]
    public void Rejects_missing_messages()
    {
        var resource = ValidResource();
        resource.Remove(ItemErrors.NotFound.Code);
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ErrorMessageProvider.FromJson(resource.ToJsonString(), Registry));
        Assert.Contains(ItemErrors.NotFound.Code, exception.Message);
    }

    [Fact]
    public void Rejects_unknown_messages()
    {
        var resource = ValidResource();
        resource["unknown.error"] = new JsonObject { ["title"] = "Title", ["detail"] = "Detail" };
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ErrorMessageProvider.FromJson(resource.ToJsonString(), Registry));
        Assert.Contains("unknown.error", exception.Message);
    }

    [Fact]
    public void Rejects_duplicate_keys_before_dictionary_deserialization()
    {
        var json = ValidResource().ToJsonString();
        var entry = JsonSerializer.Serialize(ItemErrors.NotFound.Code) + ":{\"title\":\"Duplicate\",\"detail\":\"Detail\"}";
        json = json.Insert(json.Length - 1, "," + entry);
        var exception = Assert.Throws<InvalidOperationException>(() => ErrorMessageProvider.FromJson(json, Registry));
        Assert.Contains("Duplicate", exception.Message);
        Assert.Contains(ItemErrors.NotFound.Code, exception.Message);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"title\":\"\",\"detail\":\"Detail\"}")]
    [InlineData("{\"title\":\"Title\",\"detail\":\" \"}")]
    [InlineData("{\"title\":null,\"detail\":\"Detail\"}")]
    [InlineData("{\"title\":42,\"detail\":\"Detail\"}")]
    [InlineData("[]")]
    public void Rejects_incomplete_or_invalid_entries(string entry)
    {
        var resource = ValidResource();
        resource[ItemErrors.NotFound.Code] = JsonNode.Parse(entry);
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ErrorMessageProvider.FromJson(resource.ToJsonString(), Registry));
        Assert.Contains(ItemErrors.NotFound.Code, exception.Message);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    public void Rejects_invalid_documents(string json)
    {
        Assert.Throws<InvalidOperationException>(() => ErrorMessageProvider.FromJson(json, Registry));
    }

    [Fact]
    public void Invalid_resource_fails_during_service_registration()
    {
        var directory = Directory.CreateTempSubdirectory("minioasis-errors-");
        try
        {
            Directory.CreateDirectory(Path.Combine(directory.FullName, "Resources"));
            File.WriteAllText(Path.Combine(directory.FullName, "Resources", "errors.en.json"), "{}");
            Assert.Throws<InvalidOperationException>(() =>
                new ServiceCollection().AddApiErrorHandling(directory.FullName));
        }
        finally
        {
            directory.Delete(true);
        }
    }

    private static JsonObject ValidResource()
    {
        var resource = new JsonObject();
        foreach (var code in ClientErrorCodes)
        {
            resource[code] = new JsonObject { ["title"] = "Title", ["detail"] = "Detail {id}" };
        }
        return resource;
    }
}
