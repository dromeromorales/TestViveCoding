# Specification Pattern Implementation

## 🎯 Overview

The Specification Pattern encapsulates query logic as business rules, allowing for reusable, composable, and testable query specifications.

## 🚫 The Problem We Solved

### Before: LINQ in Domain (Bad Practice)
```csharp
// ❌ LINQ directly in domain method - violates DDD
public async Task<decimal> GetPreviouslyAssignedOEFSumAsync(Guid mechanismId)
{
    var assignedOEFs = await repository.GetAsync(
        oef => oef.MechanismAssignedOef.State == MechanismAssignedOefState.Confirmed &&
               oef.State != INACTIVATED_ASSIGNED_OEF &&
               oef.StartDateYearCharge >= startPeriod &&
               // ... complex LINQ expression
    );
}
```

**Problems:**
- 🚫 Domain coupled to infrastructure (LINQ)
- 🚫 Query logic not reusable
- 🚫 Hard to test in isolation
- 🚫 Violates hexagonal architecture

## ✅ Our Solution: Domain Specifications

### Architecture Overview
```
Domain Layer
├── Specifications/
│   ├── ISpecification<T>           # Contract
│   ├── BaseSpecification<T>        # Base implementation
│   └── ProductSpecifications       # Business query factory
│
Infrastructure Layer
└── Specifications/
    └── SpecificationEvaluator<T>   # Translates to EF Core
```

## 🏗️ Implementation Details

### 1. Core Interface
```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}
```

### 2. Base Implementation
```csharp
public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    // Fluent API for building specifications
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}
```

### 3. Business Specifications Factory
```csharp
public static class ProductSpecifications
{
    public static ISpecification<Product> ByPriceRange(decimal minPrice, decimal maxPrice)
    {
        return new ProductByPriceRangeSpecification(minPrice, maxPrice);
    }

    public static ISpecification<Product> ByWeightRange(decimal minWeight, decimal maxWeight)
    {
        return new ProductByWeightRangeSpecification(minWeight, maxWeight);
    }

    public static ISpecification<Product> ByNameOrDescription(string searchTerm)
    {
        return new ProductByNameOrDescriptionSpecification(searchTerm);
    }

    // Business-meaningful combinations
    public static ISpecification<Product> AffordableAndLightweight(decimal maxPrice, decimal maxWeight)
    {
        return new ProductAffordableAndLightweightSpecification(maxPrice, maxWeight);
    }
}
```

### 4. Concrete Specification Example
```csharp
internal class ProductByPriceRangeSpecification : BaseSpecification<Product>
{
    public ProductByPriceRangeSpecification(decimal minPrice, decimal maxPrice)
        : base(p => p.Price >= minPrice && p.Price <= maxPrice)
    {
        ApplyOrderBy(p => p.Price);
    }
}
```

### 5. Infrastructure Translation
```csharp
public static class SpecificationEvaluator<T>
{
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
    {
        var query = inputQuery;

        // Apply filtering
        if (specification.Criteria != null)
            query = query.Where(specification.Criteria);

        // Apply includes for eager loading
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
            query = query.OrderBy(specification.OrderBy);
        else if (specification.OrderByDescending != null)
            query = query.OrderByDescending(specification.OrderByDescending);

        // Apply paging
        if (specification.IsPagingEnabled)
            query = query.Skip(specification.Skip).Take(specification.Take);

        return query;
    }
}
```

## 🎯 Usage Patterns

### In Use Cases (Clean)
```csharp
public async Task<IEnumerable<ProductResponse>> SearchExpensiveProductsAsync(decimal minPrice)
{
    var specification = ProductSpecifications.ExpensiveProducts(minPrice);
    var products = await _repository.GetBySpecificationAsync(specification);
    return products.Select(p => new ProductResponse(p.Id, p.Name, p.Price, p.Weight));
}
```

### In Repositories
```csharp
public async Task<IEnumerable<Product>> GetBySpecificationAsync(ISpecification<Product> specification)
{
    var query = SpecificationEvaluator<Product>.GetQuery(_context.Products.AsQueryable(), specification);
    return await query.ToListAsync();
}
```

## ✅ Benefits Achieved

### Business Benefits
- 🎯 **Domain-driven queries**: Business rules expressed in domain language
- 🔄 **Reusable logic**: Same specification used across multiple use cases
- 🧩 **Composable**: Specifications can be combined for complex queries

### Technical Benefits
- 🧪 **Testable**: Each specification can be unit tested in isolation
- 🏗️ **Clean Architecture**: Domain doesn't depend on infrastructure
- 🚀 **Performance**: Translates to efficient SQL via EF Core
- 🔧 **Maintainable**: Query logic centralized and well-organized

### Development Benefits
- 📖 **Readable**: Business intent clear from specification names
- 🔍 **Debuggable**: Easy to test individual query components
- 🛠️ **Extensible**: New specifications easy to add and test

## 🚀 Advanced Patterns

### Specification Composition
```csharp
public static ISpecification<Product> ExpensiveLightweightProducts()
{
    // Combine multiple specifications
    var expensive = ByPriceRange(1000, 10000);
    var lightweight = ByWeightRange(0, 5);
    return new AndSpecification<Product>(expensive, lightweight);
}
```

### Dynamic Specifications
```csharp
public static ISpecification<Product> BuildDynamicFilter(ProductFilterRequest request)
{
    var specs = new List<ISpecification<Product>>();
    
    if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        specs.Add(ByPriceRange(request.MinPrice ?? 0, request.MaxPrice ?? decimal.MaxValue));
    
    if (!string.IsNullOrEmpty(request.SearchTerm))
        specs.Add(ByNameOrDescription(request.SearchTerm));
    
    return new CompositeSpecification<Product>(specs);
}
```

## 📊 Testing Strategy

### Unit Tests (Business Logic)
```csharp
[Fact]
public void ByPriceRange_WithValidRange_ShouldFilterCorrectly()
{
    var products = GetTestProducts();
    var specification = ProductSpecifications.ByPriceRange(100, 500);
    
    var filteredProducts = products.Where(specification.Criteria.Compile()).ToList();
    
    Assert.All(filteredProducts, p => Assert.True(p.Price >= 100 && p.Price <= 500));
}
```

### Integration Tests (LINQ Translation)
```csharp
[Fact]
public async Task GetBySpecificationAsync_WithPriceFilter_ShouldReturnMatchingProducts()
{
    await SeedTestData();
    
    var specification = ProductSpecifications.ByPriceRange(500, 1500);
    var result = await _repository.GetBySpecificationAsync(specification);
    
    var products = result.ToList();
    Assert.Equal(3, products.Count);
    Assert.All(products, p => Assert.True(p.Price >= 500 && p.Price <= 1500));
}
```

This pattern successfully moved us from LINQ-coupled domain code to clean, testable, and reusable specifications!