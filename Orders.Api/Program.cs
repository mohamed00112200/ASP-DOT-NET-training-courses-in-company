using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("Products")
        ?? $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "products.db")}"));
builder.Services
    .AddApiVersioning(options =>
    {
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
        options.ReportApiVersions = true;
    })
    .AddMvc();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapControllers();

app.Run();

public partial class Program;
