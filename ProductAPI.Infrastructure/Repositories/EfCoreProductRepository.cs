using Microsoft.EntityFrameworkCore;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Repositories;
using ProductAPI.Domain.Specifications;
using ProductAPI.Infrastructure.Data;
using ProductAPI.Infrastructure.Specifications;

namespace ProductAPI.Infrastructure.Repositories;

public class EfCoreProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public EfCoreProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product> SaveAsync(Product product)
    {
        var existingProduct = await _context.Products.FindAsync(product.Id);
        
        if (existingProduct == null)
        {
            _context.Products.Add(product);
        }
        else
        {
            _context.Entry(existingProduct).CurrentValues.SetValues(product);
        }
        
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _context.Products.CountAsync();
        
        var products = await _context.Products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    // Specification-based methods
    public async Task<IEnumerable<Product>> GetBySpecificationAsync(ISpecification<Product> specification)
    {
        var query = SpecificationEvaluator<Product>.GetQuery(_context.Products.AsQueryable(), specification);
        return await query.ToListAsync();
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetBySpecificationWithPaginationAsync(
        ISpecification<Product> specification, 
        int pageNumber, 
        int pageSize)
    {
        var query = _context.Products.AsQueryable();
        
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        var totalCount = await query.CountAsync();
        
        var specWithPaging = new CombinedSpecification<Product>(specification, pageNumber, pageSize);
        var finalQuery = SpecificationEvaluator<Product>.GetQuery(_context.Products.AsQueryable(), specWithPaging);
        var products = await finalQuery.ToListAsync();

        return (products, totalCount);
    }

    public async Task<int> CountBySpecificationAsync(ISpecification<Product> specification)
    {
        var query = _context.Products.AsQueryable();
        
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        return await query.CountAsync();
    }

}

// Helper class for combining specifications with pagination
internal class CombinedSpecification<T> : BaseSpecification<T>
{
    public CombinedSpecification(ISpecification<T> baseSpec, int pageNumber, int pageSize)
        : base(baseSpec.Criteria)
    {
        if (baseSpec.OrderBy != null)
            ApplyOrderBy(baseSpec.OrderBy);
        else if (baseSpec.OrderByDescending != null)
            ApplyOrderByDescending(baseSpec.OrderByDescending);

        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}