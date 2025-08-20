using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Specifications;

namespace ProductAPI.Tests.Specifications;

public class ProductSpecificationsUnitTests
{
    [Fact]
    public void ByPriceRange_ShouldCreateCorrectSpecification()
    {
        var minPrice = 100m;
        var maxPrice = 500m;
        
        var specification = ProductSpecifications.ByPriceRange(minPrice, maxPrice);
        
        Assert.NotNull(specification);
        Assert.NotNull(specification.Criteria);
        Assert.NotNull(specification.OrderBy);
        Assert.False(specification.IsPagingEnabled);
    }

    [Fact]
    public void ByPriceRange_ShouldFilterCorrectly()
    {
        var products = CreateTestProducts();
        var specification = ProductSpecifications.ByPriceRange(500m, 1500m);
        
        var compiledCriteria = specification.Criteria.Compile();
        var filteredProducts = products.Where(compiledCriteria).ToList();
        
        Assert.Equal(3, filteredProducts.Count); // Gaming Laptop (1500), Office Laptop (800), Smartphone (1200)
        Assert.All(filteredProducts, p => Assert.True(p.Price >= 500m && p.Price <= 1500m));
    }

    [Fact]
    public void ByWeightRange_ShouldFilterCorrectly()
    {
        var products = CreateTestProducts();
        var specification = ProductSpecifications.ByWeightRange(1m, 5m);
        
        var compiledCriteria = specification.Criteria.Compile();
        var filteredProducts = products.Where(compiledCriteria).ToList();
        
        Assert.Equal(2, filteredProducts.Count);
        Assert.All(filteredProducts, p => Assert.True(p.Weight >= 1m && p.Weight <= 5m));
    }

    [Fact]
    public void ByNameOrDescription_ShouldFilterCorrectly()
    {
        var products = CreateTestProducts();
        var specification = ProductSpecifications.ByNameOrDescription("Gaming");
        
        var compiledCriteria = specification.Criteria.Compile();
        var filteredProducts = products.Where(compiledCriteria).ToList();
        
        Assert.Equal(2, filteredProducts.Count); // Gaming Laptop and Desktop PC (both contain "Gaming")
        Assert.All(filteredProducts, p => 
            Assert.True(p.Name.Contains("Gaming") || p.Description.Contains("Gaming")));
    }

    [Fact]
    public void ExpensiveProducts_ShouldFilterCorrectly()
    {
        var products = CreateTestProducts();
        var specification = ProductSpecifications.ExpensiveProducts(1000m);
        
        var compiledCriteria = specification.Criteria.Compile();
        var filteredProducts = products.Where(compiledCriteria).ToList();
        
        Assert.Equal(3, filteredProducts.Count); // Gaming Laptop (1500), Smartphone (1200), Desktop PC (2000)
        Assert.All(filteredProducts, p => Assert.True(p.Price >= 1000m));
    }

    [Fact]
    public void ExpensiveProducts_ShouldHaveDescendingOrder()
    {
        var specification = ProductSpecifications.ExpensiveProducts(1000m);
        
        Assert.NotNull(specification.OrderByDescending);
        Assert.Null(specification.OrderBy);
    }

    [Fact]
    public void LightweightProducts_ShouldFilterCorrectly()
    {
        var products = CreateTestProducts();
        var specification = ProductSpecifications.LightweightProducts(5m);
        
        var compiledCriteria = specification.Criteria.Compile();
        var filteredProducts = products.Where(compiledCriteria).ToList();
        
        Assert.Equal(3, filteredProducts.Count);
        Assert.All(filteredProducts, p => Assert.True(p.Weight <= 5m));
    }

    [Fact]
    public void LightweightProducts_ShouldHaveAscendingOrder()
    {
        var specification = ProductSpecifications.LightweightProducts(5m);
        
        Assert.NotNull(specification.OrderBy);
        Assert.Null(specification.OrderByDescending);
    }

    [Fact]
    public void AffordableAndLightweight_ShouldFilterCorrectly()
    {
        var products = CreateTestProducts();
        var specification = ProductSpecifications.AffordableAndLightweight(1000m, 5m);
        
        var compiledCriteria = specification.Criteria.Compile();
        var filteredProducts = products.Where(compiledCriteria).ToList();
        
        Assert.Single(filteredProducts);
        Assert.True(filteredProducts.First().Price <= 1000m);
        Assert.True(filteredProducts.First().Weight <= 5m);
    }

    [Fact]
    public void WithPagination_ShouldEnablePaging()
    {
        var specification = ProductSpecifications.WithPagination(2, 10);
        
        Assert.True(specification.IsPagingEnabled);
        Assert.Equal(10, specification.Skip); // (2-1) * 10
        Assert.Equal(10, specification.Take);
        Assert.NotNull(specification.OrderBy);
    }

    [Fact]
    public void WithPagination_ShouldCalculateSkipCorrectly()
    {
        var specification = ProductSpecifications.WithPagination(3, 5);
        
        Assert.Equal(10, specification.Skip); // (3-1) * 5 = 10
        Assert.Equal(5, specification.Take);
    }

    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 10, 10)]
    [InlineData(3, 5, 10)]
    [InlineData(5, 20, 80)]
    public void WithPagination_ShouldCalculateSkipCorrectly_ForVariousInputs(int pageNumber, int pageSize, int expectedSkip)
    {
        var specification = ProductSpecifications.WithPagination(pageNumber, pageSize);
        
        Assert.Equal(expectedSkip, specification.Skip);
        Assert.Equal(pageSize, specification.Take);
    }

    private static List<Product> CreateTestProducts()
    {
        return new List<Product>
        {
            Product.Create(Guid.NewGuid(), "Gaming Laptop", "High-performance gaming laptop", 1500m, 3.5m),
            Product.Create(Guid.NewGuid(), "Office Laptop", "Standard office laptop", 800m, 2.1m),
            Product.Create(Guid.NewGuid(), "Smartphone", "Latest smartphone model", 1200m, 0.2m),
            Product.Create(Guid.NewGuid(), "Desktop PC", "Gaming desktop computer", 2000m, 15m)
        };
    }
}