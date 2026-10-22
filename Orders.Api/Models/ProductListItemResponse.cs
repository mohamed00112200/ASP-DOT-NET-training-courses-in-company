namespace Orders.Api.Models;

public sealed record ProductListItemResponse(
    int Id,
    string Name,
    decimal Price,
    int CategoryId,
    string CategoryName,
    Guid ConcurrencyToken,
    DateTime CreatedAt,
    DateTime UpdatedAt);
