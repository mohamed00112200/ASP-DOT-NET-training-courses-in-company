using Orders.Application.Abstractions;
using Orders.Application.Cqrs;

namespace Orders.Application.Products;

public sealed record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;

public sealed class GetProductByIdQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        return product is null
            ? null
            : new ProductDto(product.Id, product.Name, product.Price);
    }
}
