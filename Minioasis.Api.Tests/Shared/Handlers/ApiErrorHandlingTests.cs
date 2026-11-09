using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Minioasis.Api.Tests.Shared.TestDoubles;
using Minioasis.Api.Tests.TestInfrastructure.Shared;
using Minioasis.Application.Abstractions.Item;
using Minioasis.Application.Errors.Shared;
using Minioasis.Application.Models.Item;
using Minioasis.Application.Results.Shared;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Minioasis.Api.Tests.Shared.Handlers;

public sealed class ApiErrorHandlingTests
{
    [Theory]
    [InlineData("catalog.invalid_response", HttpStatusCode.BadGateway)]
    [InlineData("catalog.unavailable", HttpStatusCode.ServiceUnavailable)]
    [InlineData("catalog.authentication_failed", HttpStatusCode.InternalServerError)]
    [InlineData("system.unexpected", HttpStatusCode.InternalServerError)]
    public async Task Masks_server_failures_and_logs_internal_code_with_response_trace_id(string code, HttpStatusCode status)
    {
        var logs = new RecordingLoggerProvider();
        var error = new ApplicationError(ApplicationErrorDefinitionRegistry.Discover().Definitions[code],
            new Dictionary<string, string> { ["id"] = "private-provider-diagnostic" });
        await using var factory = CreateFactory(
            (_, _) => Task.FromResult(ApplicationResult<ItemDetails>.Failure(error)), logs);
        using var response = await factory.CreateClient().GetAsync("/api/v1/items/12", TestContext.Current.CancellationToken);

        var traceId = await AssertInternalError(response, status);
        Assert.Contains(logs.Entries, entry => entry.Message.Contains(code, StringComparison.Ordinal)
            && entry.Message.Contains(traceId, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task Masks_unhandled_exceptions_and_logs_exception_with_response_trace_id(string environment)
    {
        var logs = new RecordingLoggerProvider();
        var exception = new InvalidOperationException("private-provider-diagnostic");
        await using var factory = CreateFactory((_, _) => throw exception, logs);
        await using var configured = factory.WithWebHostBuilder(builder => builder.UseEnvironment(environment));
        using var response = await configured.CreateClient().GetAsync("/api/v1/items/12", TestContext.Current.CancellationToken);

        var traceId = await AssertInternalError(response, HttpStatusCode.InternalServerError);
        Assert.Contains(logs.Entries, entry => ReferenceEquals(exception, entry.Exception)
            && entry.Message.Contains(traceId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Propagates_caller_cancellation_without_logging_an_internal_failure()
    {
        var logs = new RecordingLoggerProvider();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var factory = CreateFactory(async (_, cancellationToken) =>
        {
            started.SetResult();
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
                throw new InvalidOperationException("The request should have been cancelled.");
            }
            finally
            {
                cancelled.SetResult();
            }
        }, logs);
        using var client = factory.CreateClient();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var request = client.GetAsync("/api/v1/items/12", cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request);
        await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(logs.Entries, entry => entry.Message.Contains("Unhandled API failure", StringComparison.Ordinal)
            || entry.Message.Contains("Application failure", StringComparison.Ordinal));
    }

    private static async Task<string> AssertInternalError(HttpResponseMessage response, HttpStatusCode status)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.Equal(new[] { "code", "status", "title", "traceId" }, problem.Keys.Order(StringComparer.Ordinal));
        Assert.Equal("Internal error", problem["title"].ToString());
        Assert.Equal("internal_error", problem["code"].ToString());
        Assert.Equal(((int)status).ToString(), problem["status"].ToString());
        var traceId = problem["traceId"].ToString();
        Assert.False(string.IsNullOrWhiteSpace(traceId));
        return traceId!;
    }

    private static ApiFactory CreateFactory(
        Func<GetItemQuery, CancellationToken, Task<ApplicationResult<ItemDetails>>> execute,
        RecordingLoggerProvider logs) => new(services =>
        {
            services.RemoveAll<IGetItemUseCase>();
            services.AddSingleton<IGetItemUseCase>(new StubGetItemUseCase(execute));
            services.AddSingleton<ILoggerProvider>(logs);
        });

    private sealed class StubGetItemUseCase(
        Func<GetItemQuery, CancellationToken, Task<ApplicationResult<ItemDetails>>> execute) : IGetItemUseCase
    {
        public Task<ApplicationResult<ItemDetails>> ExecuteAsync(GetItemQuery query,
            CancellationToken cancellationToken = default) => execute(query, cancellationToken);
    }
}
