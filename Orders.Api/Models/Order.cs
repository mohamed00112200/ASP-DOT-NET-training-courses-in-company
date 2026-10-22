namespace Orders.Api.Models;

public sealed record Order(int Id, string Name, decimal Price, string Currency);
