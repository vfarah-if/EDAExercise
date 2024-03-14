using System.ComponentModel.DataAnnotations;

namespace Domain.Contracts;

public record TestMessage
{
    [Required]
    public Guid CommandId { get; init; }

    [Required]
    public DateTime Created { get; init; } = DateTime.Now;

    [Required]
    public string? Identifier { get; init; }
}
