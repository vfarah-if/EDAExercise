using System.ComponentModel.DataAnnotations;

namespace Domain.Contracts;

public record ManifestDetail
{
    [Required]
    [MinLength(6)]
    public string? SourceSystemId { get; init; }

    [Required]
    [MinLength(24)]
    public string? EventId { get; init; }

    [Required]
    public DateTimeOffset Timestamp { get; init; }

    [Required]
    [MinLength(6)]
    public string? PurchaseOrderNumber { get; init; }

    [Required]
    [Range(1, 1000)]
    public int DeliveryLocation { get; init; }

    [Required]
    [MinItems(1)]
    [UniqueItems]
    public List<ProductDetail>? Products { get; init; }
}
