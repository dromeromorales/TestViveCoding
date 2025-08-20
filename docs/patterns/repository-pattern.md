# Repository Pattern Implementation

## 🎯 Overview

The Repository Pattern provides an abstraction layer between the domain and data access layers, enabling clean separation of concerns and improved testability.

## 🏗️ Architecture

```
Domain Layer
└── IProductRepository          # Contract (Port)

Infrastructure Layer  
├── EfCoreProductRepository     # EF Core implementation (Adapter)
└── InMemoryProductRepository   # In-memory implementation (Adapter)
```

## 📋 Interface Design

### Clean Contract
```csharp
public interface IProductRepository
{
    // Basic CRUD operations
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> SaveAsync(Product product);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
    
    // Specification-based queries
    Task<IEnumerable<Product>> GetBySpecificationAsync(ISpecification<Product> specification);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetBySpecificationWithPaginationAsync(
        ISpecification<Product> specification, int pageNumber, int pageSize);
    Task<int> CountBySpecificationAsync(ISpecification<Product> specification);
}
```

## 🚀 Benefits Achieved

### Testability
- **Unit tests**: Use mocks of `IProductRepository`
- **Integration tests**: Use real implementations with test data
- **Flexible testing**: Can swap implementations easily

### Clean Architecture
- **Domain independence**: Domain layer doesn't know about EF Core
- **Infrastructure flexibility**: Can swap database technologies
- **Dependency inversion**: Domain defines contract, infrastructure implements

### Maintainability  
- **Single responsibility**: Each implementation handles one data store
- **Open/closed principle**: Easy to add new implementations
- **Clear contracts**: Interface defines expected behavior

## 🔧 Implementation Details

### EF Core Implementation
```csharp
public class EfCoreProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public async Task<Product> SaveAsync(Product product)
    {
        var existingProduct = await _context.Products.FindAsync(product.Id);
        
        if (existingProduct == null)
            _context.Products.Add(product);
        else
            _context.Entry(existingProduct).CurrentValues.SetValues(product);
        
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<IEnumerable<Product>> GetBySpecificationAsync(ISpecification<Product> specification)
    {
        var query = SpecificationEvaluator<Product>.GetQuery(_context.Products.AsQueryable(), specification);
        return await query.ToListAsync();
    }
}
```

### In-Memory Implementation  
```csharp
public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    public Task<Product> SaveAsync(Product product)
    {
        _products.AddOrUpdate(product.Id, product, (key, oldValue) => product);
        return Task.FromResult(product);
    }

    public Task<IEnumerable<Product>> GetBySpecificationAsync(ISpecification<Product> specification)
    {
        var query = _products.Values.AsQueryable();

        if (specification.Criteria != null)
        {
            var compiledCriteria = specification.Criteria.Compile();
            query = query.Where(compiledCriteria).AsQueryable();
        }

        return Task.FromResult(query.AsEnumerable());
    }
}
```

## 🧪 Testing Strategy

### Unit Tests (With Mocks)
```csharp
[Fact]
public async Task ExecuteAsync_WithValidRequest_ShouldCallRepository()
{
    var mockRepository = new Mock<IProductRepository>();
    var useCase = new CreateProductUseCase(mockRepository.Object);
    
    await useCase.ExecuteAsync(validRequest);
    
    mockRepository.Verify(r => r.SaveAsync(It.IsAny<Product>()), Times.Once);
}
```

### Integration Tests (With Real Implementation)
```csharp
[Fact] 
public async Task SaveAsync_ThenGetByIdAsync_ShouldReturnSavedProduct()
{
    var context = GetInMemoryContext();
    var repository = new EfCoreProductRepository(context);
    
    var product = Product.Create(Guid.NewGuid(), "Test", "Description", 100m, 1m);
    await repository.SaveAsync(product);
    
    var retrieved = await repository.GetByIdAsync(product.Id);
    Assert.Equal(product.Id, retrieved?.Id);
}
```

## ⚖️ Trade-offs Made

### Chosen Approach: Simple Repository + Specifications
**Pros:**
- ✅ Clean separation of concerns
- ✅ Easy to test and mock
- ✅ Specifications handle complex queries
- ✅ Repository stays focused on data access

**Cons:**
- ❌ More abstractions than direct data access
- ❌ Potential performance overhead

### Alternative Considered: Rich Repository with Query Methods
```csharp
// ❌ Not chosen - would have many specific methods
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal min, decimal max);
    Task<IEnumerable<Product>> GetByWeightRangeAsync(decimal min, decimal max);
    Task<IEnumerable<Product>> SearchByNameAsync(string name);
    // ... many more methods
}
```

**Why rejected:**
- 🚫 Interface explosion with many query methods
- 🚫 Hard to compose complex queries
- 🚫 Business logic leaks into repository interface

## 🔄 Evolution History

### Phase 1: Legacy LINQ Methods (Removed)
```csharp
// ❌ Had these methods - caused coupling issues
Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal min, decimal max);
Task<IEnumerable<Product>> SearchByNameAsync(string searchTerm);
```

### Phase 2: Current Clean Specification-Based
```csharp
// ✅ Current approach - clean and flexible
Task<IEnumerable<Product>> GetBySpecificationAsync(ISpecification<Product> specification);
```

**Migration benefits:**
- 🎯 Moved query logic to domain specifications
- 🧪 Made unit testing more focused  
- 🔧 Reduced repository interface complexity
- 🚀 Enabled query composition and reuse

## 🚀 Future Enhancements

### Async Streaming
```csharp
IAsyncEnumerable<Product> GetBySpecificationStreamAsync(ISpecification<Product> specification);
```

### Bulk Operations
```csharp
Task SaveRangeAsync(IEnumerable<Product> products);
Task DeleteRangeAsync(IEnumerable<Guid> productIds);
```

### Caching Layer
```csharp
public class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _inner;
    private readonly IMemoryCache _cache;
    
    // Decorator pattern for caching
}
```

This repository pattern provides a solid foundation for data access while maintaining clean architecture principles!