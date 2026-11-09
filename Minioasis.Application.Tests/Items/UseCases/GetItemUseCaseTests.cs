using Microsoft.Extensions.DependencyInjection;
using Minioasis.Application.Items.Abstractions;
using Minioasis.Application.Items.Identifiers;
using Minioasis.Application.Items.Requests;
using Minioasis.Application.Items.Responses;
using Minioasis.Application.Items.Results;
using Minioasis.Application.Shared.Errors;
using Minioasis.Application.Shared.Extensions;
using Xunit;

namespace Minioasis.Application.Tests.Items.UseCases;

public sealed class GetItemUseCaseTests
{
    [Fact]
    public async Task Returns_get_item_response_when_gateway_finds_item()
    {
        var item = new GetItemResponse("12", "34", "ABC", "QA1");
        var useCase = Create(GetItemGatewayResult.Found(item));
        var result = await useCase.ExecuteAsync(new GetItemRequest("12"), TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.Equal(item, result.Value);
    }

    [Theory]
    [InlineData(
        GetItemOutcome.NotFound,
        "items.not_found",
        ApplicationErrorCategory.NotFound)]
    [InlineData(
        GetItemOutcome.InvalidIdentifier,
        "items.invalid_identifier",
        ApplicationErrorCategory.Validation)]
    [InlineData(
        GetItemOutcome.Unavailable,
        "items.get.unavailable",
        ApplicationErrorCategory.DependencyUnavailable)]
    [InlineData(
        GetItemOutcome.AuthenticationFailed,
        "items.get.authentication_failed",
        ApplicationErrorCategory.DependencyAuthentication)]
    [InlineData(
        GetItemOutcome.InvalidResponse,
        "items.get.invalid_response",
        ApplicationErrorCategory.DependencyInvalidResponse)]
    public async Task Translates_gateway_outcomes(
        GetItemOutcome outcome,
        string code,
        ApplicationErrorCategory category)
    {
        var useCase = Create(Result(outcome));
        var result = await useCase.ExecuteAsync(new GetItemRequest("12"), TestContext.Current.CancellationToken);
        Assert.False(result.IsSuccess);
        Assert.Equal(code, result.Error!.Code);
        Assert.Equal(category, result.Error.Category);
    }

    [Fact]
    public async Task Rejects_blank_identifier_without_calling_gateway()
    {
        var gateway = new FakeGetItemGateway(GetItemGatewayResult.NotFound());
        var useCase = Create(gateway);
        var result = await useCase.ExecuteAsync(new GetItemRequest("  "), TestContext.Current.CancellationToken);
        Assert.Equal("items.invalid_identifier", result.Error!.Code);
        Assert.Equal(0, gateway.Calls);
    }

    private static IGetItemUseCase Create(GetItemGatewayResult result) => Create(new FakeGetItemGateway(result));

    private static IGetItemUseCase Create(FakeGetItemGateway gateway)
    {
        var services = new ServiceCollection()
            .AddSingleton<IGetItemGateway>(gateway)
            .AddMinioasisApplication()
            .BuildServiceProvider();

        return services.GetRequiredService<IGetItemUseCase>();
    }

    private static GetItemGatewayResult Result(GetItemOutcome outcome) =>
        outcome switch
        {
            GetItemOutcome.NotFound => GetItemGatewayResult.NotFound(),
            GetItemOutcome.InvalidIdentifier => GetItemGatewayResult.InvalidIdentifier(),
            GetItemOutcome.Unavailable => GetItemGatewayResult.Unavailable(),
            GetItemOutcome.AuthenticationFailed => GetItemGatewayResult.AuthenticationFailed(),
            _ => GetItemGatewayResult.InvalidResponse()
        };

    private sealed class FakeGetItemGateway(GetItemGatewayResult result) : IGetItemGateway
    {
        public int Calls
        {
            get; private set;
        }

        public Task<GetItemGatewayResult> GetItemAsync(
            ItemId itemId,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(result);
        }
    }
}
