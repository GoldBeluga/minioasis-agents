namespace Minioasis.Koha.Authentication.Abstractions;

internal interface IKohaAccessTokenProvider
{
    Task<string> GetAsync(CancellationToken cancellationToken);
    void Invalidate();
}
