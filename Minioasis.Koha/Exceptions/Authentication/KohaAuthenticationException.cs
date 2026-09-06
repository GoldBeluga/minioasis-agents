namespace Minioasis.Koha.Exceptions.Authentication;

internal sealed class KohaAuthenticationException(string message, Exception? inner = null)
    : Exception(message, inner);
