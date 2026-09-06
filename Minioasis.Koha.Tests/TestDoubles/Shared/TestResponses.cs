using System.Net;

namespace Minioasis.Koha.Tests.TestDoubles.Shared;

internal static class TestResponses
{
    public static Task<HttpResponseMessage> Json(HttpStatusCode status, string json) =>
        Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        });
}
