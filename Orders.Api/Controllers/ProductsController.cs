using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Models;
using Orders.Api.Services;

namespace Orders.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/products")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    [MapToApiVersion(1.0)]
    [ProducesResponseType<ProductV1Response>(StatusCodes.Status200OK)]
    public ActionResult<ProductV1Response> GetV1()
    {
        var product = productService.GetProduct();
        return Ok(new ProductV1Response(product.Id, product.Name, product.Price));
    }

    [HttpGet]
    [MapToApiVersion(2.0)]
    [ProducesResponseType<ProductV2Response>(StatusCodes.Status200OK)]
    public ActionResult<ProductV2Response> GetV2()
    {
        var product = productService.GetProduct();
        return Ok(new ProductV2Response(
            product.Id,
            product.Name,
            product.Price,
            product.CategoryName));
    }
}
