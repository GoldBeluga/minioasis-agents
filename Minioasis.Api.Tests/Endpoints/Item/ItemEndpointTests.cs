using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minioasis.Api.Tests.TestDoubles.Item;
using Minioasis.Api.Tests.TestInfrastructure.Shared;
using Minioasis.Application.Abstractions.Item;
using Minioasis.Application.Errors.Shared;
using Minioasis.Application.Models.Item;
using Minioasis.Application.Results.Shared;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Minioasis.Api.Tests.Endpoints.Item;

public sealed class ItemEndpointTests
{
    [Fact]
    public async Task Returns_item_json_from_application_use_case()
    {
        await using var factory = CreateFactory(
            ApplicationResult<ItemDetails>.Success(new("12", "34", "ABC", "QA1")));
        using var response = await factory.CreateClient().GetAsync("/api/v1/items/12", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var item = await response.Content.ReadFromJsonAsync<ItemDetails>(TestContext.Current.CancellationToken);
        Assert.Equal(new ItemDetails("12", "34", "ABC", "QA1"), item);
    }

    [Theory]
    [InlineData(
        ApplicationErrorCategory.Validation,
        "items.invalid_identifier",
        HttpStatusCode.BadRequest,
        "Invalid item identifier",
        "The item identifier must be a valid positive identifier.")]
    [InlineData(
        ApplicationErrorCategory.NotFound,
        "items.not_found",
        HttpStatusCode.NotFound,
        "Item not found",
        "No item was found with identifier 12.")]
    public async Task Maps_application_errors_to_problem_details(
        ApplicationErrorCategory category,
        string code,
        HttpStatusCode status,
        string title,
        string detail)
    {
        var error = new ApplicationError(
            ApplicationErrorDefinitionRegistry.Discover().Definitions[code],
            code == "items.not_found"
                ? new Dictionary<string, string> { ["id"] = "12" }
                : null);
        await using var factory = CreateFactory(ApplicationResult<ItemDetails>.Failure(error));
        using var response = await factory.CreateClient().GetAsync("/api/v1/items/12", TestContext.Current.CancellationToken);
        Assert.Equal(status, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(TestContext.Current.CancellationToken);
        Assert.Equal(code, problem!["code"].ToString());
        Assert.False(problem.ContainsKey("type"));
        Assert.False(problem.ContainsKey("instance"));
        Assert.Equal(category, error.Category);
        Assert.Equal(title, problem["title"].ToString());
        Assert.Equal(detail, problem["detail"].ToString());
        Assert.Equal(((int)status).ToString(), problem["status"].ToString());
        Assert.False(string.IsNullOrWhiteSpace(problem["traceId"].ToString()));
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }

    private static ApiFactory CreateFactory(ApplicationResult<ItemDetails> result) =>
        new(services =>
        {
            services.RemoveAll<IGetItemUseCase>();
            services.AddSingleton<IGetItemUseCase>(new FakeGetItemUseCase(result));
        });
}
