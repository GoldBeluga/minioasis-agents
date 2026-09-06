using System.Net;

namespace Minioasis.Koha.Tests.TestDoubles.Authentication;

internal sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;

    public void Advance(TimeSpan duration) => utcNow = utcNow.Add(duration);
}
