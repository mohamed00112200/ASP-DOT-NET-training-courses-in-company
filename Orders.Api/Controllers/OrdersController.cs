using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Models;
using Orders.Api.Services;

namespace Orders.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/orders")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet("{id:int}")]
    [MapToApiVersion(1.0)]
    [ProducesResponseType<OrderV1Response>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OrderV1Response> GetV1(int id)
    {
        var order = orderService.GetById(id);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(new OrderV1Response(order.Id, order.Name, order.Price));
    }

    [HttpGet("{id:int}")]
    [MapToApiVersion(2.0)]
    [ProducesResponseType<OrderV2Response>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OrderV2Response> GetV2(int id)
    {
        var order = orderService.GetById(id);
        if (order is null)
        {
            return NotFound();
        }

        var price = new MoneyResponse(order.Price, order.Currency);
        return Ok(new OrderV2Response(order.Id, order.Name, price));
    }
}
