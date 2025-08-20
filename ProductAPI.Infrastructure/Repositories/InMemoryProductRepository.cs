using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Repositories;
using ProductAPI.Domain.Specifications;
using System.Collections.Concurrent;

namespace ProductAPI.Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    public Task<Product?> GetByIdAsync(Guid id)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<Product> SaveAsync(Product product)
    {
        _products.AddOrUpdate(product.Id, product, (key, oldValue) => product);
        return Task.FromResult(product);
    }

    public Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        var allProducts = _products.Values.ToList();
        var totalCount = allProducts.Count;
        
        var skip = (pageNumber - 1) * pageSize;
        var pagedProducts = allProducts
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        return Task.FromResult((pagedProducts.AsEnumerable(), totalCount));
    }

    // Specification-based methods
    public Task<IEnumerable<Product>> GetBySpecificationAsync(ISpecification<Product> specification)
    {
        var query = _products.Values.AsQueryable();

        if (specification.Criteria != null)
        {
            var compiledCriteria = specification.Criteria.Compile();
            query = query.Where(compiledCriteria).AsQueryable();
        }

        if (specification.OrderBy != null)
        {
            var compiledOrderBy = specification.OrderBy.Compile();
            query = query.OrderBy(compiledOrderBy).AsQueryable();
        }
        else if (specification.OrderByDescending != null)
        {
            var compiledOrderByDesc = specification.OrderByDescending.Compile();
            query = query.OrderByDescending(compiledOrderByDesc).AsQueryable();
        }

        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return Task.FromResult(query.AsEnumerable());
    }

    public Task<(IEnumerable<Product> Products, int TotalCount)> GetBySpecificationWithPaginationAsync(
        ISpecification<Product> specification, 
        int pageNumber, 
        int pageSize)
    {
        var query = _products.Values.AsQueryable();

        if (specification.Criteria != null)
        {
            var compiledCriteria = specification.Criteria.Compile();
            query = query.Where(compiledCriteria).AsQueryable();
        }

        var totalCount = query.Count();

        if (specification.OrderBy != null)
        {
            var compiledOrderBy = specification.OrderBy.Compile();
            query = query.OrderBy(compiledOrderBy).AsQueryable();
        }
        else if (specification.OrderByDescending != null)
        {
            var compiledOrderByDesc = specification.OrderByDescending.Compile();
            query = query.OrderByDescending(compiledOrderByDesc).AsQueryable();
        }

        var skip = (pageNumber - 1) * pageSize;
        var pagedProducts = query.Skip(skip).Take(pageSize).ToList();

        return Task.FromResult((pagedProducts.AsEnumerable(), totalCount));
    }

    public Task<int> CountBySpecificationAsync(ISpecification<Product> specification)
    {
        var query = _products.Values.AsQueryable();

        if (specification.Criteria != null)
        {
            var compiledCriteria = specification.Criteria.Compile();
            query = query.Where(compiledCriteria).AsQueryable();
        }

        return Task.FromResult(query.Count());
    }

}