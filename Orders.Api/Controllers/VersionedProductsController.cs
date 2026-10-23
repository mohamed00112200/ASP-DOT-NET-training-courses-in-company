using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Models;
using Orders.Infrastructure.Data;

namespace Orders.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/products")]
public sealed class VersionedProductsController(ProductsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [MapToApiVersion(1.0)]
    [ProducesResponseType<ProductV1Response>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductV1Response>> GetV1(CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .Select(product => new ProductV1Response(
                product.Id,
                product.Name,
                product.Price))
            .FirstOrDefaultAsync(cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet]
    [MapToApiVersion(2.0)]
    [ProducesResponseType<ProductV2Response>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductV2Response>> GetV2(CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .Select(product => new ProductV2Response(
                product.Id,
                product.Name,
                product.Price,
                product.Category.Name))
            .FirstOrDefaultAsync(cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }
}
