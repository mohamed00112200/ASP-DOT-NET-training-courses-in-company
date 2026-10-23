using System.ComponentModel.DataAnnotations;

namespace Orders.Api.Models;

public sealed class CreateProductRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    [RegularExpression(@".*\S.*")]
    public required string Name { get; init; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Price { get; init; }
}
