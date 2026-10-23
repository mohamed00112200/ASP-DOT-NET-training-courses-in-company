using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Models;
using Orders.Application.Cqrs;
using Orders.Application.Products;
using Orders.Domain.Entities;
using Orders.Infrastructure.Data;

namespace Orders.Api.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/products")]
public sealed class AdvancedEfCoreProductsController(
    ProductsDbContext dbContext,
    IRequestDispatcher requestDispatcher,
    ILogger<AdvancedEfCoreProductsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<ProductListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProductListItemResponse>>> GetProducts(
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var skip = ((long)pageNumber - 1) * pageSize;
        if (skip > int.MaxValue)
        {
            return BadRequest("The requested page is outside the supported range.");
        }

        var result = await requestDispatcher.Send<
            GetProductsQuery,
            PagedProductsDto<ProductListItemDto>>(
            new GetProductsQuery(pageNumber, pageSize, search),
            cancellationToken);
        var response = new PagedResponse<ProductListItemResponse>(
            result.Items
                .Select(item => new ProductListItemResponse(
                    item.Id,
                    item.Name,
                    item.Price,
                    item.CategoryId,
                    item.CategoryName,
                    item.ConcurrencyToken,
                    item.CreatedAt,
                    item.UpdatedAt))
                .ToList(),
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductById(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await requestDispatcher.Send<GetProductByIdQuery, ProductDto?>(
            new GetProductByIdQuery(id),
            cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await requestDispatcher.Send<CreateProductCommand, ProductDto>(
            new CreateProductCommand(request.Name, request.Price),
            cancellationToken);

        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<ProductListItemResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductListItemResponse>> UpdateProduct(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ConcurrencyToken == Guid.Empty)
        {
            return BadRequest("A non-empty concurrencyToken is required.");
        }

        var product = await dbContext.Products
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        var categoryName = await dbContext.Categories
            .Where(category => category.Id == request.CategoryId)
            .Select(category => category.Name)
            .SingleOrDefaultAsync(cancellationToken);
        if (categoryName is null)
        {
            return BadRequest("The specified category does not exist.");
        }

        dbContext.Entry(product)
            .Property(item => item.ConcurrencyToken)
            .OriginalValue = request.ConcurrencyToken;

        product.Name = request.Name;
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;
        product.ConcurrencyToken = Guid.NewGuid();

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogInformation(exception, "Product {ProductId} changed during update.", id);
            return Conflict(new ProblemDetails
            {
                Title = "The product was changed by another request.",
                Detail = "Reload the product and retry your update with its latest concurrencyToken.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var response = new ProductListItemResponse(
            product.Id,
            product.Name,
            product.Price,
            product.CategoryId,
            categoryName,
            product.ConcurrencyToken,
            product.CreatedAt,
            product.UpdatedAt);

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
