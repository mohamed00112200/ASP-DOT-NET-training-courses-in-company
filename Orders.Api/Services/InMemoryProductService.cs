using Orders.Api.Models;

namespace Orders.Api.Services;

public sealed class InMemoryProductService : IProductService
{
    private static readonly Product Laptop = new(1, "Laptop", 25000m, "Electronics");

    public Product GetProduct() => Laptop;
}
