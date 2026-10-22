namespace Orders.Api.Models;

public sealed record ProductV2Response(int Id, string Name, decimal Price, string CategoryName);
