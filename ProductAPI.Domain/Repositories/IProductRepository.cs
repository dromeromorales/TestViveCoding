using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Specifications;

namespace ProductAPI.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> SaveAsync(Product product);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
    
    // Specification-based methods
    Task<IEnumerable<Product>> GetBySpecificationAsync(ISpecification<Product> specification);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetBySpecificationWithPaginationAsync(
        ISpecification<Product> specification, 
        int pageNumber, 
        int pageSize);
    Task<int> CountBySpecificationAsync(ISpecification<Product> specification);
}