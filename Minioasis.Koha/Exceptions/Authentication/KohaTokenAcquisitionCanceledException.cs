namespace Minioasis.Koha.Exceptions.Authentication;

internal sealed class KohaTokenAcquisitionCanceledException(OperationCanceledException inner)
    : OperationCanceledException("Koha OAuth token acquisition was canceled.", inner);
