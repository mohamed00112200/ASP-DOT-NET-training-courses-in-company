using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Cqrs;
using Orders.Application.Products;

namespace Orders.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRequestDispatcher, RequestDispatcher>();
        services.AddScoped<
            IRequestHandler<GetProductsQuery, PagedProductsDto<ProductListItemDto>>,
            GetProductsQueryHandler>();
        services.AddScoped<
            IRequestHandler<GetProductByIdQuery, ProductDto?>,
            GetProductByIdQueryHandler>();
        services.AddScoped<
            IRequestHandler<CreateProductCommand, ProductDto>,
            CreateProductCommandHandler>();

        return services;
    }
}
