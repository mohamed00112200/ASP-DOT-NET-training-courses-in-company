namespace Orders.Application.Products;

public sealed record PagedProductsDto<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
