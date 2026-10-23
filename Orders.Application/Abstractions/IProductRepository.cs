using Orders.Domain.Entities;

namespace Orders.Application.Abstractions;

public interface IProductRepository
{
    Task<int> CountAsync(string? search, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetPageAsync(
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
