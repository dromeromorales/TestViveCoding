using ProductAPI.Application.DTOs;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Repositories;

namespace ProductAPI.Application.UseCases;

public class CreateProductUseCase
{
    private readonly IProductRepository _productRepository;

    public CreateProductUseCase(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductResponse> ExecuteAsync(CreateProductRequest request)
    {
        var productId = Guid.NewGuid();
        
        var product = Product.Create(
            productId,
            request.Name,
            request.Description,
            request.Price,
            request.Weight
        );

        var savedProduct = await _productRepository.SaveAsync(product);

        return new ProductResponse(
            savedProduct.Id,
            savedProduct.Name,
            savedProduct.Description,
            savedProduct.Price,
            savedProduct.Weight
        );
    }
}