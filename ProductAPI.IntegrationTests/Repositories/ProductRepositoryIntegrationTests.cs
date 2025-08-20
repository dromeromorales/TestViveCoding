using Microsoft.EntityFrameworkCore;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Specifications;
using ProductAPI.Infrastructure.Data;
using ProductAPI.Infrastructure.Repositories;

namespace ProductAPI.IntegrationTests.Repositories;

public class ProductRepositoryIntegrationTests : IDisposable
{
    private readonly ProductDbContext _context;
    private readonly EfCoreProductRepository _repository;

    public ProductRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductDbContext(options);
        _repository = new EfCoreProductRepository(_context);
    }

    [Fact]
    public async Task GetBySpecificationAsync_WithPriceFilter_ShouldReturnMatchingProducts()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByPriceRange(500, 1500);
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Equal(3, products.Count); // Gaming Laptop (1500), Office Laptop (800), Smartphone (1200)
        Assert.All(products, p => Assert.True(p.Price >= 500 && p.Price <= 1500));
    }

    [Fact]
    public async Task GetBySpecificationAsync_WithWeightFilter_ShouldReturnMatchingProducts()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByWeightRange(2.1m, 9.9m);
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Equal(2, products.Count);
        Assert.All(products, p => Assert.True(p.Weight >= 2.1m && p.Weight <= 9.9m));
    }

    [Fact]
    public async Task GetBySpecificationAsync_WithComplexFilter_ShouldReturnMatchingProducts()
    {
        await SeedTestData();

        var specification = ProductSpecifications.AffordableAndLightweight(2000m, 5m);
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Equal(3, products.Count); // Gaming Laptop (1500, 3.5), Office Laptop (800, 2.1), and Smartphone (1200, 0.2)
        Assert.All(products, p => Assert.True(p.Price <= 2000m && p.Weight <= 5m));
    }

    [Fact]
    public async Task GetBySpecificationWithPaginationAsync_ShouldReturnCorrectPageAndCount()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByPriceRange(100, 5000);
        var (products, totalCount) = await _repository.GetBySpecificationWithPaginationAsync(
            specification, 1, 2);

        Assert.Equal(2, products.Count());
        Assert.Equal(4, totalCount);
    }

    [Fact]
    public async Task GetByPriceRangeSpecification_ShouldReturnProductsInRange()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByPriceRange(500, 1500);
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Equal(3, products.Count); // Gaming Laptop (1500), Office Laptop (800), Smartphone (1200)
        Assert.All(products, p => Assert.True(p.Price >= 500 && p.Price <= 1500));
    }

    [Fact]
    public async Task GetByWeightRangeSpecification_ShouldReturnProductsInRange()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByWeightRange(1, 5);
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Equal(2, products.Count); // Gaming Laptop (3.5), Office Laptop (2.1)
        Assert.All(products, p => Assert.True(p.Weight >= 1 && p.Weight <= 5));
    }

    [Fact]
    public async Task SearchByNameSpecification_ShouldReturnMatchingProducts()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByNameOrDescription("Smartphone");
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Single(products);
        Assert.Contains("Smartphone", products.First().Name);
    }

    [Fact]
    public async Task SearchByDescriptionSpecification_ShouldReturnMatchingProducts()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByNameOrDescription("gaming");
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Single(products); // Only Desktop PC has "gaming" in description
        Assert.All(products, p => 
            Assert.True(p.Name.Contains("gaming", StringComparison.OrdinalIgnoreCase) || 
                       p.Description.Contains("gaming", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task GetBySpecificationAsync_WithNoMatches_ShouldReturnEmptyList()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByPriceRange(5000, 10000);
        var result = await _repository.GetBySpecificationAsync(specification);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBySpecificationAsync_WithCaseInsensitiveNameSearch_ShouldReturnMatches()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByNameOrDescription("laptop");
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Equal(2, products.Count);
    }

    [Fact]
    public async Task SaveAsync_ThenGetBySpecificationAsync_ShouldFindSavedProduct()
    {
        var newProduct = Product.Create(
            Guid.NewGuid(),
            "Test Product",
            "Test Description",
            999m,
            3m
        );

        await _repository.SaveAsync(newProduct);

        var specification = ProductSpecifications.ByNameOrDescription("Test Product");
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Single(products);
        Assert.Equal(newProduct.Id, products.First().Id);
    }

    private async Task SeedTestData()
    {
        var products = new[]
        {
            Product.Create(Guid.NewGuid(), "Gaming Laptop", "High-performance gaming laptop", 1500m, 3.5m),
            Product.Create(Guid.NewGuid(), "Office Laptop", "Standard office laptop", 800m, 2.1m),
            Product.Create(Guid.NewGuid(), "Smartphone", "Latest smartphone model", 1200m, 0.2m),
            Product.Create(Guid.NewGuid(), "Desktop PC", "Gaming desktop computer", 2000m, 15m)
        };

        foreach (var product in products)
        {
            await _repository.SaveAsync(product);
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}