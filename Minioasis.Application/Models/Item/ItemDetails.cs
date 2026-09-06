namespace Minioasis.Application.Models.Item;

public sealed record ItemDetails(
    string Id,
    string BibliographicRecordId,
    string? Barcode,
    string? CallNumber);
