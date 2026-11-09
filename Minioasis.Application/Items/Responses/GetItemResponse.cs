namespace Minioasis.Application.Items.Responses;

public sealed record GetItemResponse(
    string Id,
    string BibliographicRecordId,
    string? Barcode,
    string? CallNumber);
