using Minioasis.Application.Errors.Catalog;
using Minioasis.Application.Errors.Item;
using Minioasis.Application.Errors.Shared;
using Minioasis.Application.Errors.System;
using Xunit;

namespace Minioasis.Application.Tests.Errors.Shared;

public sealed class ApplicationErrorDefinitionRegistryTests
{
    [Fact]
    public void Discovers_production_groups()
    {
        var definitions = ApplicationErrorDefinitionRegistry.Discover().Definitions;
        Assert.Same(ItemErrors.NotFound, definitions[ItemErrors.NotFound.Code]);
        Assert.Same(CatalogErrors.Unavailable, definitions[CatalogErrors.Unavailable.Code]);
        Assert.Same(SystemErrors.Unexpected, definitions[SystemErrors.Unexpected.Code]);
    }

    [Fact]
    public void Discovers_marked_groups_only_and_compares_codes_ordinally()
    {
        var registry = ApplicationErrorDefinitionRegistry.Discover(
            [typeof(FirstGroup), typeof(SecondGroup), typeof(UnmarkedGroup)]);
        Assert.Equal(2, registry.Definitions.Count);
        Assert.Contains("test.error", registry.Definitions.Keys);
        Assert.Contains("TEST.ERROR", registry.Definitions.Keys);
    }

    [Theory]
    [InlineData(typeof(DuplicateGroup), "Duplicate", "test.error")]
    [InlineData(typeof(BlankGroup), "blank", "BlankGroup")]
    [InlineData(typeof(InvalidCategoryGroup), "invalid category", "invalid.category")]
    [InlineData(typeof(NullGroup), "null", "NullGroup")]
    public void Rejects_invalid_definitions(Type group, string reason, string identifier)
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ApplicationErrorDefinitionRegistry.Discover([typeof(FirstGroup), group]));
        Assert.Contains(reason, exception.Message);
        Assert.Contains(identifier, exception.Message);
    }

    [Fact]
    public void Rejects_empty_discovery()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ApplicationErrorDefinitionRegistry.Discover([typeof(UnmarkedGroup)]));
    }

    [Fact]
    public void Application_error_derives_code_and_category_from_definition()
    {
        var error = new ApplicationError(ItemErrors.NotFound, new Dictionary<string, string> { ["id"] = "12" });
        Assert.Same(ItemErrors.NotFound, error.Definition);
        Assert.Equal(ItemErrors.NotFound.Code, error.Code);
        Assert.Equal(ItemErrors.NotFound.Category, error.Category);
        Assert.Equal("12", error.Metadata!["id"]);
        Assert.Throws<ArgumentNullException>(() => new ApplicationError(null!));
    }

    [ErrorDefinitions]
    private static class FirstGroup
    {
        public static readonly ApplicationErrorDefinition Error = new("test.error", ApplicationErrorCategory.Validation);
        public static ApplicationErrorDefinition Mutable = new("ignored.mutable", ApplicationErrorCategory.Validation);
        public static ApplicationErrorDefinition Property => new("ignored.property", ApplicationErrorCategory.Validation);
    }

    [ErrorDefinitions]
    private static class SecondGroup
    {
        public static readonly ApplicationErrorDefinition Error = new("TEST.ERROR", ApplicationErrorCategory.NotFound);
    }

    private static class UnmarkedGroup
    {
        public static readonly ApplicationErrorDefinition Error = new("ignored", ApplicationErrorCategory.Unexpected);
    }

    [ErrorDefinitions]
    private static class DuplicateGroup
    {
        public static readonly ApplicationErrorDefinition Error = new("test.error", ApplicationErrorCategory.NotFound);
    }

    [ErrorDefinitions]
    private static class BlankGroup
    {
        public static readonly ApplicationErrorDefinition Error = new(" ", ApplicationErrorCategory.Validation);
    }

    [ErrorDefinitions]
    private static class InvalidCategoryGroup
    {
        public static readonly ApplicationErrorDefinition Error = new("invalid.category", (ApplicationErrorCategory)999);
    }

    [ErrorDefinitions]
    private static class NullGroup
    {
        public static readonly ApplicationErrorDefinition Error = null!;
    }
}
