using Microsoft.EntityFrameworkCore;
using ProductAPI.Application.DTOs;
using ProductAPI.Application.UseCases;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Specifications;
using ProductAPI.Infrastructure.Data;
using ProductAPI.Infrastructure.Repositories;

namespace ProductAPI.IntegrationTests.UseCases;

public class ProductUseCaseIntegrationTests : IDisposable
{
    private readonly ProductDbContext _context;
    private readonly EfCoreProductRepository _repository;
    private readonly CreateProductUseCase _createUseCase;
    private readonly GetAllProductsUseCase _getAllUseCase;

    public ProductUseCaseIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductDbContext(options);
        _repository = new EfCoreProductRepository(_context);
        _createUseCase = new CreateProductUseCase(_repository);
        _getAllUseCase = new GetAllProductsUseCase(_repository);
    }

    [Fact]
    public async Task CreateProduct_ThenGetAll_ShouldReturnCreatedProduct()
    {
        var createRequest = new CreateProductRequest(
            "Integration Test Product",
            "Product created in integration test",
            500m,
            2.5m
        );

        var createdProduct = await _createUseCase.ExecuteAsync(createRequest);

        var paginationRequest = new PaginationRequest(1, 10);
        var getAllResult = await _getAllUseCase.ExecuteAsync(paginationRequest);

        Assert.Single(getAllResult.Items);
        var retrievedProduct = getAllResult.Items.First();
        
        Assert.Equal(createdProduct.Id, retrievedProduct.Id);
        Assert.Equal(createdProduct.Name, retrievedProduct.Name);
        Assert.Equal(createdProduct.Description, retrievedProduct.Description);
        Assert.Equal(createdProduct.Price, retrievedProduct.Price);
        Assert.Equal(createdProduct.Weight, retrievedProduct.Weight);
    }

    [Fact]
    public async Task CreateMultipleProducts_ThenGetAllWithPagination_ShouldReturnCorrectPage()
    {
        for (int i = 1; i <= 15; i++)
        {
            var createRequest = new CreateProductRequest(
                $"Product {i}",
                $"Description {i}",
                100m + i,
                1m + i
            );
            await _createUseCase.ExecuteAsync(createRequest);
        }

        var paginationRequest = new PaginationRequest(2, 5);
        var result = await _getAllUseCase.ExecuteAsync(paginationRequest);

        Assert.Equal(5, result.Items.Count());
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task CreateProductsWithDifferentPrices_FilterByPriceRange_ShouldReturnCorrectProducts()
    {
        var products = new[]
        {
            new CreateProductRequest("Cheap Product", "Low price", 50m, 1m),
            new CreateProductRequest("Medium Product", "Medium price", 500m, 2m),
            new CreateProductRequest("Expensive Product", "High price", 1500m, 3m),
            new CreateProductRequest("Very Expensive Product", "Very high price", 3000m, 4m)
        };

        foreach (var product in products)
        {
            await _createUseCase.ExecuteAsync(product);
        }

        var specification = ProductSpecifications.ByPriceRange(400m, 1000m);
        var mediumPriceProducts = await _repository.GetBySpecificationAsync(specification);

        var result = mediumPriceProducts.ToList();
        Assert.Single(result);
        Assert.Equal("Medium Product", result.First().Name);
        Assert.Equal(500m, result.First().Price);
    }

    [Fact]
    public async Task CreateProductsWithDifferentWeights_FilterByWeightRange_ShouldReturnCorrectProducts()
    {
        var products = new[]
        {
            new CreateProductRequest("Light Product", "Light weight", 100m, 0.5m),
            new CreateProductRequest("Medium Weight Product", "Medium weight", 200m, 5m),
            new CreateProductRequest("Heavy Product", "Heavy weight", 300m, 15m),
            new CreateProductRequest("Very Heavy Product", "Very heavy", 400m, 50m)
        };

        foreach (var product in products)
        {
            await _createUseCase.ExecuteAsync(product);
        }

        var specification = ProductSpecifications.ByWeightRange(3m, 20m);
        var mediumWeightProducts = await _repository.GetBySpecificationAsync(specification);

        var result = mediumWeightProducts.ToList();
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Medium Weight Product");
        Assert.Contains(result, p => p.Name == "Heavy Product");
    }

    [Fact]
    public async Task CreateProducts_SearchByName_ShouldReturnMatchingProducts()
    {
        var products = new[]
        {
            new CreateProductRequest("Gaming Laptop Pro", "Professional gaming laptop", 1500m, 3m),
            new CreateProductRequest("Office Laptop Basic", "Basic office laptop", 800m, 2m),
            new CreateProductRequest("Gaming Desktop", "Gaming desktop computer", 2000m, 15m),
            new CreateProductRequest("Office Desktop", "Office desktop computer", 1000m, 12m)
        };

        foreach (var product in products)
        {
            await _createUseCase.ExecuteAsync(product);
        }

        var specification = ProductSpecifications.ByNameOrDescription("Gaming");
        var gamingProducts = await _repository.GetBySpecificationAsync(specification);

        var result = gamingProducts.ToList();
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Contains("Gaming", p.Name));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}