using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Orders.Api.Tests;

public sealed class ProductsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetV1_ReturnsLegacyProductShape()
    {
        using var response = await _client.GetAsync("/api/v1/products");
        using var body = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(body);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, root.GetProperty("id").GetInt32());
        Assert.Equal("Laptop", root.GetProperty("name").GetString());
        Assert.Equal(25000m, root.GetProperty("price").GetDecimal());
        Assert.False(root.TryGetProperty("categoryName", out _));
    }

    [Fact]
    public async Task GetV2_ReturnsProductWithCategoryName()
    {
        using var response = await _client.GetAsync("/api/v2/products");
        using var body = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(body);
        var root = json.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, root.GetProperty("id").GetInt32());
        Assert.Equal("Laptop", root.GetProperty("name").GetString());
        Assert.Equal(25000m, root.GetProperty("price").GetDecimal());
        Assert.Equal("Electronics", root.GetProperty("categoryName").GetString());
    }
}
