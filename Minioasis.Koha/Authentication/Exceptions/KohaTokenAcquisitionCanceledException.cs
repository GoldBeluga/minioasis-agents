namespace Minioasis.Koha.Authentication.Exceptions;

internal sealed class KohaTokenAcquisitionCanceledException(OperationCanceledException inner)
    : OperationCanceledException("Koha OAuth token acquisition was canceled.", inner);
