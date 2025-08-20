using Microsoft.EntityFrameworkCore;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Specifications;
using ProductAPI.Infrastructure.Data;
using ProductAPI.Infrastructure.Repositories;

namespace ProductAPI.IntegrationTests.Specifications;

public class ProductSpecificationsIntegrationTests : IDisposable
{
    private readonly ProductDbContext _context;
    private readonly EfCoreProductRepository _repository;

    public ProductSpecificationsIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductDbContext(options);
        _repository = new EfCoreProductRepository(_context);
    }

    [Fact]
    public async Task ByPriceRange_ShouldReturnProductsInRange_WithCorrectOrdering()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByPriceRange(500m, 1500m);
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Equal(3, products.Count);
        
        // Verify ordering by price (ascending)
        Assert.True(products.SequenceEqual(products.OrderBy(p => p.Price)));
        
        // Verify all products are in range
        Assert.All(products, p => Assert.True(p.Price >= 500m && p.Price <= 1500m));
    }

    [Fact]
    public async Task ByWeightRange_ShouldReturnProductsInRange_WithCorrectOrdering()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByWeightRange(1m, 5m);
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Equal(2, products.Count);
        
        // Verify ordering by weight (ascending)
        Assert.True(products.SequenceEqual(products.OrderBy(p => p.Weight)));
        
        // Verify all products are in range
        Assert.All(products, p => Assert.True(p.Weight >= 1m && p.Weight <= 5m));
    }

    [Fact]
    public async Task ByNameOrDescription_ShouldReturnMatchingProducts_WithCorrectOrdering()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByNameOrDescription("Gaming");
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Equal(2, products.Count); // Gaming Laptop and Desktop PC (both contain "Gaming")
        
        // Verify ordering by name (ascending)
        Assert.True(products.SequenceEqual(products.OrderBy(p => p.Name)));
        
        // Verify all products match
        Assert.All(products, p => 
            Assert.True(p.Name.Contains("Gaming") || p.Description.Contains("Gaming")));
    }

    [Fact]
    public async Task ExpensiveProducts_ShouldReturnProductsAboveThreshold_OrderedByPriceDescending()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ExpensiveProducts(1000m);
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Equal(3, products.Count);
        
        // Verify ordering by price (descending)
        Assert.True(products.SequenceEqual(products.OrderByDescending(p => p.Price)));
        
        // Verify all products are above threshold
        Assert.All(products, p => Assert.True(p.Price >= 1000m));
    }

    [Fact]
    public async Task LightweightProducts_ShouldReturnProductsBelowWeight_OrderedByWeightAscending()
    {
        await SeedTestData();

        var specification = ProductSpecifications.LightweightProducts(5m);
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Equal(3, products.Count);
        
        // Verify ordering by weight (ascending)
        Assert.True(products.SequenceEqual(products.OrderBy(p => p.Weight)));
        
        // Verify all products are below weight threshold
        Assert.All(products, p => Assert.True(p.Weight <= 5m));
    }

    [Fact]
    public async Task AffordableAndLightweight_ShouldReturnProductsMatchingBothCriteria()
    {
        await SeedTestData();

        var specification = ProductSpecifications.AffordableAndLightweight(1000m, 5m);
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Single(products); // Only Office Laptop (800 <= 1000, 2.1 <= 5) qualifies
        
        // Verify ordering by price (ascending)
        Assert.True(products.SequenceEqual(products.OrderBy(p => p.Price)));
        
        // Verify all products meet both criteria
        Assert.All(products, p => 
        {
            Assert.True(p.Price <= 1000m);
            Assert.True(p.Weight <= 5m);
        });
    }

    [Fact]
    public async Task WithPagination_ShouldReturnCorrectPageSize()
    {
        await SeedTestData();

        var specification = ProductSpecifications.WithPagination(1, 2);
        var result = await _repository.GetBySpecificationAsync(specification);

        var products = result.ToList();
        Assert.Equal(2, products.Count);
        
        // Verify ordering by name (ascending) - default for pagination spec
        Assert.True(products.SequenceEqual(products.OrderBy(p => p.Name)));
    }

    [Fact]
    public async Task SpecificationWithPagination_ShouldReturnCorrectCountAndPage()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ByPriceRange(500m, 2000m);
        var (products, totalCount) = await _repository.GetBySpecificationWithPaginationAsync(
            specification, 1, 2);

        Assert.Equal(2, products.Count());
        Assert.Equal(4, totalCount); // All 4 products are in range 500-2000: Gaming Laptop (1500), Office Laptop (800), Smartphone (1200), Desktop PC (2000)
    }

    [Fact]
    public async Task CountBySpecification_ShouldReturnCorrectCount()
    {
        await SeedTestData();

        var specification = ProductSpecifications.ExpensiveProducts(1000m);
        var count = await _repository.CountBySpecificationAsync(specification);

        Assert.Equal(3, count);
    }

    [Fact]
    public async Task CombinedSpecifications_ShouldWorkCorrectly()
    {
        await SeedTestData();

        // Test chaining multiple specifications conceptually
        var expensiveSpec = ProductSpecifications.ExpensiveProducts(800m);
        var expensiveProducts = await _repository.GetBySpecificationAsync(expensiveSpec);
        
        var lightweightSpec = ProductSpecifications.LightweightProducts(10m);
        var lightweightProducts = await _repository.GetBySpecificationAsync(lightweightSpec);

        // Verify both specs return different results
        Assert.Equal(4, expensiveProducts.Count()); // All products >= 800
        Assert.Equal(3, lightweightProducts.Count()); // Products <= 10kg
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