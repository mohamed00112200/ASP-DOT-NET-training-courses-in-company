using Orders.Api.Models;

namespace Orders.Api.Services;

public interface IOrderService
{
    Order? GetById(int id);
}
