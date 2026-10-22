namespace Orders.Api.Models;

public sealed record Product(int Id, string Name, decimal Price, string CategoryName);
