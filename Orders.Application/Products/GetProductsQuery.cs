using Orders.Application.Abstractions;
using Orders.Application.Cqrs;

namespace Orders.Application.Products;

public sealed record GetProductsQuery(
    int PageNumber,
    int PageSize,
    string? Search) : IRequest<PagedProductsDto<ProductListItemDto>>;

public sealed class GetProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductsQuery, PagedProductsDto<ProductListItemDto>>
{
    public async Task<PagedProductsDto<ProductListItemDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
        var totalCount = await productRepository.CountAsync(search, cancellationToken);
        var skip = (int)(((long)request.PageNumber - 1) * request.PageSize);
        var products = await productRepository.GetPageAsync(
            search,
            skip,
            request.PageSize,
            cancellationToken);
        var items = products
            .Select(product => new ProductListItemDto(
                product.Id,
                product.Name,
                product.Price,
                product.CategoryId,
                product.Category.Name,
                product.ConcurrencyToken,
                product.CreatedAt,
                product.UpdatedAt))
            .ToList();

        return new PagedProductsDto<ProductListItemDto>(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }
}
