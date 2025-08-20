using ProductAPI.Application.DTOs;
using ProductAPI.Domain.Repositories;

namespace ProductAPI.Application.UseCases;

public class GetAllProductsUseCase
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsUseCase(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResponse<ProductResponse>> ExecuteAsync(PaginationRequest request)
    {
        var (products, totalCount) = await _productRepository.GetAllAsync(request.PageNumber, request.PageSize);

        var productResponses = products.Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Weight
        ));

        return PagedResponse<ProductResponse>.Create(
            productResponses,
            request.PageNumber,
            request.PageSize,
            totalCount
        );
    }
}