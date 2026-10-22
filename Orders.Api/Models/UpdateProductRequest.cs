using System.ComponentModel.DataAnnotations;

namespace Orders.Api.Models;

public sealed class UpdateProductRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Name { get; init; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Price { get; init; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }

    public Guid ConcurrencyToken { get; init; }
}
