using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Orders.Api.Services;
using Xunit;

namespace Orders.Api.Tests;

public sealed class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetV1_ReturnsPriceAsDecimal()
    {
        using var response = await _client.GetAsync("/api/v1/orders/1");
        using var body = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(body);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Laptop", root.GetProperty("name").GetString());
        Assert.Equal(25000.50m, root.GetProperty("price").GetDecimal());
        Assert.False(root.TryGetProperty("currency", out _));
    }

    [Fact]
    public async Task GetV2_ReturnsPriceAsAmountAndCurrency()
    {
        using var response = await _client.GetAsync("/api/v2/orders/1");
        using var body = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(body);
        var root = json.RootElement;
        var price = root.GetProperty("price");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Laptop", root.GetProperty("name").GetString());
        Assert.Equal(25000.50m, price.GetProperty("amount").GetDecimal());
        Assert.Equal("EGP", price.GetProperty("currency").GetString());
    }

    [Fact]
    public async Task GetOrder_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        using var response = await _client.GetAsync("/api/v2/orders/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void InMemoryOrderService_ReturnsNullWhenOrderDoesNotExist()
    {
        var service = new InMemoryOrderService();

        var order = service.GetById(999);

        Assert.Null(order);
    }
}
