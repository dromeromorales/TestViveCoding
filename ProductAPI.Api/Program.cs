using Microsoft.EntityFrameworkCore;
using ProductAPI.Application.UseCases;
using ProductAPI.Domain.Repositories;
using ProductAPI.Infrastructure.Data;
using ProductAPI.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Product API",
        Version = "v1",
        Description = "API for managing products with hexagonal architecture and DDD",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Product API Team"
        }
    });
});

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseInMemoryDatabase("ProductsDb"));

builder.Services.AddScoped<IProductRepository, EfCoreProductRepository>();
builder.Services.AddScoped<CreateProductUseCase>();
builder.Services.AddScoped<GetAllProductsUseCase>();
builder.Services.AddScoped<SearchProductsUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
