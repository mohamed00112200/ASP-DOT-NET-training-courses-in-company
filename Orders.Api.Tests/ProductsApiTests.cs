using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Orders.Api.Tests;

public sealed class ProductsApiTests : IDisposable
{
    private readonly TestApiFactory _factory = new();
    private readonly HttpClient _client;

    public ProductsApiTests()
    {
        _client = _factory.CreateClient();
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

    [Fact]
    public async Task GetProducts_ReturnsPagedItemsWithCategoryAndSearch()
    {
        using var response = await _client.GetAsync("/api/products?pageNumber=2&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = json.RootElement;
        var items = root.GetProperty("items");

        Assert.Equal(4, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, root.GetProperty("totalPages").GetInt32());
        Assert.Equal(2, items.GetArrayLength());
        Assert.Equal("Smartphone", items[0].GetProperty("name").GetString());
        Assert.Equal("Books", items[1].GetProperty("categoryName").GetString());
        Assert.True(items[0].TryGetProperty("concurrencyToken", out _));
        Assert.True(items[0].TryGetProperty("createdAt", out _));
        Assert.True(items[0].TryGetProperty("updatedAt", out _));

        using var searchResponse = await _client.GetAsync("/api/products?search=Lap");
        using var searchJson = await JsonDocument.ParseAsync(
            await searchResponse.Content.ReadAsStreamAsync());
        var searchRoot = searchJson.RootElement;
        Assert.Equal(1, searchRoot.GetProperty("totalCount").GetInt32());
        Assert.Equal("Laptop", searchRoot.GetProperty("items")[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task UpdateProduct_RejectsStaleTokenAndDeleteHidesProduct()
    {
        using var listResponse = await _client.GetAsync("/api/products?pageSize=1");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var listJson = await JsonDocument.ParseAsync(
            await listResponse.Content.ReadAsStreamAsync());
        var item = listJson.RootElement.GetProperty("items")[0];
        var id = item.GetProperty("id").GetInt32();
        var originalToken = item.GetProperty("concurrencyToken").GetGuid();
        var originalUpdatedAt = item.GetProperty("updatedAt").GetDateTime();

        var update = new
        {
            name = "Laptop Pro",
            price = 27000m,
            categoryId = 1,
            concurrencyToken = originalToken
        };
        using var updateResponse = await _client.PutAsJsonAsync($"/api/products/{id}", update);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        using var updatedJson = await JsonDocument.ParseAsync(
            await updateResponse.Content.ReadAsStreamAsync());
        var updatedProduct = updatedJson.RootElement;
        var updatedToken = updatedProduct.GetProperty("concurrencyToken").GetGuid();
        Assert.NotEqual(originalToken, updatedToken);
        Assert.Equal(
            item.GetProperty("createdAt").GetDateTime(),
            updatedProduct.GetProperty("createdAt").GetDateTime());
        Assert.True(updatedProduct.GetProperty("updatedAt").GetDateTime() > originalUpdatedAt);

        using var staleResponse = await _client.PutAsJsonAsync($"/api/products/{id}", update);
        Assert.Equal(HttpStatusCode.Conflict, staleResponse.StatusCode);

        using var deleteResponse = await _client.DeleteAsync($"/api/products/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var finalResponse = await _client.GetAsync("/api/products");
        using var finalJson = await JsonDocument.ParseAsync(
            await finalResponse.Content.ReadAsStreamAsync());
        Assert.Equal(3, finalJson.RootElement.GetProperty("totalCount").GetInt32());
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
