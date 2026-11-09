using Minioasis.Koha.Authentication.Abstractions;
using System.Net;

namespace Minioasis.Koha.Tests.Authentication.TestDoubles;

internal sealed class SequenceKohaAccessTokenProvider(params string[] values) : IKohaAccessTokenProvider
{
    private int index;

    public int GetCalls
    {
        get; private set;
    }

    public int Invalidations
    {
        get; private set;
    }

    public Task<string> GetAsync(CancellationToken cancellationToken)
    {
        GetCalls++;
        var value = values[Math.Min(index, values.Length - 1)];
        index++;
        return Task.FromResult(value);
    }

    public void Invalidate() => Invalidations++;
}
