namespace Minioasis.Koha.Abstractions.Authentication;

internal interface IKohaAccessTokenProvider
{
    Task<string> GetAsync(CancellationToken cancellationToken);
    void Invalidate();
}
