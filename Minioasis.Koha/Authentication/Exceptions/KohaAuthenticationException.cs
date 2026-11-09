namespace Minioasis.Koha.Authentication.Exceptions;

internal sealed class KohaAuthenticationException(string message, Exception? inner = null)
    : Exception(message, inner);
