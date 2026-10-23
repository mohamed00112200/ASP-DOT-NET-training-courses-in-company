namespace Orders.Domain.Entities;

public sealed class Product : IAuditableEntity
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public Guid ConcurrencyToken { get; set; }
}
