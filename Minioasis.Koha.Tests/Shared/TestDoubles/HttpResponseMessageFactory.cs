using System.Net;

namespace Minioasis.Koha.Tests.Shared.TestDoubles;

internal static class HttpResponseMessageFactory
{
    public static Task<HttpResponseMessage> CreateJsonAsync(HttpStatusCode status, string json) =>
        Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        });
}
