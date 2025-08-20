using ProductAPI.Application.DTOs;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Repositories;
using ProductAPI.Domain.Specifications;

namespace ProductAPI.Application.UseCases;

public class SearchProductsUseCase
{
    private readonly IProductRepository _productRepository;

    public SearchProductsUseCase(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductResponse>> SearchByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var specification = ProductSpecifications.ByPriceRange(minPrice, maxPrice);
        var products = await _productRepository.GetBySpecificationAsync(specification);

        return products.Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Weight
        ));
    }

    public async Task<IEnumerable<ProductResponse>> SearchByWeightRangeAsync(decimal minWeight, decimal maxWeight)
    {
        var specification = ProductSpecifications.ByWeightRange(minWeight, maxWeight);
        var products = await _productRepository.GetBySpecificationAsync(specification);

        return products.Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Weight
        ));
    }

    public async Task<IEnumerable<ProductResponse>> SearchByNameOrDescriptionAsync(string searchTerm)
    {
        var specification = ProductSpecifications.ByNameOrDescription(searchTerm);
        var products = await _productRepository.GetBySpecificationAsync(specification);

        return products.Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Weight
        ));
    }

    public async Task<IEnumerable<ProductResponse>> GetExpensiveProductsAsync(decimal threshold = 1000m)
    {
        var specification = ProductSpecifications.ExpensiveProducts(threshold);
        var products = await _productRepository.GetBySpecificationAsync(specification);

        return products.Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Weight
        ));
    }

    public async Task<IEnumerable<ProductResponse>> GetLightweightProductsAsync(decimal maxWeight = 5m)
    {
        var specification = ProductSpecifications.LightweightProducts(maxWeight);
        var products = await _productRepository.GetBySpecificationAsync(specification);

        return products.Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Weight
        ));
    }

    public async Task<IEnumerable<ProductResponse>> GetAffordableAndLightweightProductsAsync(
        decimal maxPrice = 500m, 
        decimal maxWeight = 5m)
    {
        var specification = ProductSpecifications.AffordableAndLightweight(maxPrice, maxWeight);
        var products = await _productRepository.GetBySpecificationAsync(specification);

        return products.Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Weight
        ));
    }

    public async Task<PagedResponse<ProductResponse>> SearchWithPaginationAsync(
        ISpecification<Product> specification,
        PaginationRequest paginationRequest)
    {
        var (products, totalCount) = await _productRepository.GetBySpecificationWithPaginationAsync(
            specification, 
            paginationRequest.PageNumber, 
            paginationRequest.PageSize);

        var productResponses = products.Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Weight
        ));

        return PagedResponse<ProductResponse>.Create(
            productResponses,
            paginationRequest.PageNumber,
            paginationRequest.PageSize,
            totalCount
        );
    }
}