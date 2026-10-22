using Asp.Versioning;
using Orders.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IProductService, InMemoryProductService>();
builder.Services
    .AddApiVersioning(options =>
    {
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
        options.ReportApiVersions = true;
    })
    .AddMvc();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program;
