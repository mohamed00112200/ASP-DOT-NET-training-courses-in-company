using Orders.Application.Abstractions;
using Orders.Application.Cqrs;
using Orders.Domain.Entities;

namespace Orders.Application.Products;

public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<ProductDto>;

public sealed class CreateProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            CategoryId = 1,
            ConcurrencyToken = Guid.NewGuid()
        };

        await productRepository.AddAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);

        return new ProductDto(product.Id, product.Name, product.Price);
    }
}
