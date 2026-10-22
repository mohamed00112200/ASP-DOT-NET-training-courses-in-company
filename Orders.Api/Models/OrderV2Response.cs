namespace Orders.Api.Models;

public sealed record OrderV2Response(int Id, string Name, MoneyResponse Price);
