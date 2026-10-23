using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions;
using Orders.Domain.Entities;
using Orders.Infrastructure.Data;

namespace Orders.Infrastructure.Repositories;

public sealed class ProductRepository(ProductsDbContext dbContext) : IProductRepository
{
    public Task<int> CountAsync(string? search, CancellationToken cancellationToken)
    {
        return ApplySearch(dbContext.Products, search).CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetPageAsync(
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        return await ApplySearch(dbContext.Products, search)
            .AsNoTracking()
            .Include(product => product.Category)
            .OrderBy(product => product.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Product> ApplySearch(IQueryable<Product> products, string? search)
    {
        return string.IsNullOrWhiteSpace(search)
            ? products
            : products.Where(product => product.Name.StartsWith(search));
    }
}
