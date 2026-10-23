namespace Orders.Application.Products;

public sealed record ProductListItemDto(
    int Id,
    string Name,
    decimal Price,
    int CategoryId,
    string CategoryName,
    Guid ConcurrencyToken,
    DateTime CreatedAt,
    DateTime UpdatedAt);
