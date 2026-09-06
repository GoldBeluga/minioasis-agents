using Minioasis.Application.Extensions.Shared;
using Minioasis.Application.Abstractions.Item;
using Minioasis.Application.Errors.Shared;
using Minioasis.Application.Models.Item;
using Minioasis.Application.Results.Item;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Minioasis.Application.Tests.UseCases.Item;

public sealed class GetItemUseCaseTests
{
    [Fact]
    public async Task Returns_application_item_when_gateway_finds_it()
    {
        var item = new ItemDetails("12", "34", "ABC", "QA1");
        var useCase = Create(ItemLookupResult.Found(item));
        var result = await useCase.ExecuteAsync(new GetItemQuery("12"), TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.Equal(item, result.Value);
    }

    [Theory]
    [InlineData(
        ItemLookupOutcome.NotFound,
        "items.not_found",
        ApplicationErrorCategory.NotFound)]
    [InlineData(
        ItemLookupOutcome.InvalidIdentifier,
        "items.invalid_identifier",
        ApplicationErrorCategory.Validation)]
    [InlineData(
        ItemLookupOutcome.Unavailable,
        "catalog.unavailable",
        ApplicationErrorCategory.DependencyUnavailable)]
    [InlineData(
        ItemLookupOutcome.AuthenticationFailed,
        "catalog.authentication_failed",
        ApplicationErrorCategory.DependencyAuthentication)]
    [InlineData(
        ItemLookupOutcome.InvalidResponse,
        "catalog.invalid_response",
        ApplicationErrorCategory.DependencyInvalidResponse)]
    public async Task Translates_gateway_outcomes(
        ItemLookupOutcome outcome,
        string code,
        ApplicationErrorCategory category)
    {
        var useCase = Create(Result(outcome));
        var result = await useCase.ExecuteAsync(new GetItemQuery("12"), TestContext.Current.CancellationToken);
        Assert.False(result.IsSuccess);
        Assert.Equal(code, result.Error!.Code);
        Assert.Equal(category, result.Error.Category);
    }

    [Fact]
    public async Task Rejects_blank_identifier_without_calling_gateway()
    {
        var gateway = new FakeGateway(ItemLookupResult.NotFound());
        var useCase = Create(gateway);
        var result = await useCase.ExecuteAsync(new GetItemQuery("  "), TestContext.Current.CancellationToken);
        Assert.Equal("items.invalid_identifier", result.Error!.Code);
        Assert.Equal(0, gateway.Calls);
    }

    private static IGetItemUseCase Create(ItemLookupResult result) => Create(new FakeGateway(result));

    private static IGetItemUseCase Create(FakeGateway gateway)
    {
        var services = new ServiceCollection()
            .AddSingleton<IItemCatalogGateway>(gateway)
            .AddMinioasisApplication()
            .BuildServiceProvider();

        return services.GetRequiredService<IGetItemUseCase>();
    }

    private static ItemLookupResult Result(ItemLookupOutcome outcome) =>
        outcome switch
        {
            ItemLookupOutcome.NotFound => ItemLookupResult.NotFound(),
            ItemLookupOutcome.InvalidIdentifier => ItemLookupResult.InvalidIdentifier(),
            ItemLookupOutcome.Unavailable => ItemLookupResult.Unavailable(),
            ItemLookupOutcome.AuthenticationFailed => ItemLookupResult.AuthenticationFailed(),
            _ => ItemLookupResult.InvalidResponse()
        };

    private sealed class FakeGateway(ItemLookupResult result) : IItemCatalogGateway
    {
        public int Calls
        {
            get; private set;
        }

        public Task<ItemLookupResult> FindByIdAsync(
            ExternalItemId itemId,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(result);
        }
    }
}
