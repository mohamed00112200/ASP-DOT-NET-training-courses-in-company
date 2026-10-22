using Orders.Api.Models;

namespace Orders.Api.Services;

public sealed class InMemoryOrderService : IOrderService
{
    private static readonly IReadOnlyDictionary<int, Order> Orders =
        new Dictionary<int, Order>
        {
            [1] = new Order(1, "Laptop", 25000.50m, "EGP"),
            [2] = new Order(2, "Keyboard", 1250.00m, "EGP")
        };

    public Order? GetById(int id) =>
        Orders.TryGetValue(id, out var order) ? order : null;
}
