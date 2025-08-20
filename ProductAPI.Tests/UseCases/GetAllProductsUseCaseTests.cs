using Moq;
using ProductAPI.Application.DTOs;
using ProductAPI.Application.UseCases;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Repositories;

namespace ProductAPI.Tests.UseCases;

public class GetAllProductsUseCaseTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly GetAllProductsUseCase _useCase;

    public GetAllProductsUseCaseTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _useCase = new GetAllProductsUseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidPagination_ShouldReturnPagedProducts()
    {
        var products = CreateTestProducts(15);
        var paginationRequest = new PaginationRequest(1, 10);
        var expectedProducts = products.Take(10).ToList();

        _mockRepository.Setup(x => x.GetAllAsync(1, 10))
                      .ReturnsAsync((expectedProducts, 15));

        var result = await _useCase.ExecuteAsync(paginationRequest);

        Assert.NotNull(result);
        Assert.Equal(10, result.Items.Count());
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
        
        _mockRepository.Verify(x => x.GetAllAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithSecondPage_ShouldReturnCorrectPaginationInfo()
    {
        var products = CreateTestProducts(15);
        var paginationRequest = new PaginationRequest(2, 10);
        var expectedProducts = products.Skip(10).Take(10).ToList();

        _mockRepository.Setup(x => x.GetAllAsync(2, 10))
                      .ReturnsAsync((expectedProducts, 15));

        var result = await _useCase.ExecuteAsync(paginationRequest);

        Assert.NotNull(result);
        Assert.Equal(5, result.Items.Count());
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
        
        _mockRepository.Verify(x => x.GetAllAsync(2, 10), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyRepository_ShouldReturnEmptyPagedResponse()
    {
        var paginationRequest = new PaginationRequest(1, 10);

        _mockRepository.Setup(x => x.GetAllAsync(1, 10))
                      .ReturnsAsync((new List<Product>(), 0));

        var result = await _useCase.ExecuteAsync(paginationRequest);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
        
        _mockRepository.Verify(x => x.GetAllAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithSinglePage_ShouldReturnCorrectPaginationInfo()
    {
        var products = CreateTestProducts(5);
        var paginationRequest = new PaginationRequest(1, 10);

        _mockRepository.Setup(x => x.GetAllAsync(1, 10))
                      .ReturnsAsync((products, 5));

        var result = await _useCase.ExecuteAsync(paginationRequest);

        Assert.NotNull(result);
        Assert.Equal(5, result.Items.Count());
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
        
        _mockRepository.Verify(x => x.GetAllAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomPageSize_ShouldUseCorrectPageSize()
    {
        var products = CreateTestProducts(25);
        var paginationRequest = new PaginationRequest(1, 5);
        var expectedProducts = products.Take(5).ToList();

        _mockRepository.Setup(x => x.GetAllAsync(1, 5))
                      .ReturnsAsync((expectedProducts, 25));

        var result = await _useCase.ExecuteAsync(paginationRequest);

        Assert.NotNull(result);
        Assert.Equal(5, result.Items.Count());
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(5, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
        
        _mockRepository.Verify(x => x.GetAllAsync(1, 5), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnCorrectProductResponses()
    {
        var products = CreateTestProducts(3);
        var paginationRequest = new PaginationRequest(1, 10);

        _mockRepository.Setup(x => x.GetAllAsync(1, 10))
                      .ReturnsAsync((products, 3));

        var result = await _useCase.ExecuteAsync(paginationRequest);

        var productList = result.Items.ToList();
        Assert.Equal(3, productList.Count);
        
        for (int i = 0; i < 3; i++)
        {
            var expectedProduct = products.ElementAt(i);
            var actualProduct = productList[i];
            
            Assert.Equal(expectedProduct.Id, actualProduct.Id);
            Assert.Equal(expectedProduct.Name, actualProduct.Name);
            Assert.Equal(expectedProduct.Description, actualProduct.Description);
            Assert.Equal(expectedProduct.Price, actualProduct.Price);
            Assert.Equal(expectedProduct.Weight, actualProduct.Weight);
        }
    }

    private static IEnumerable<Product> CreateTestProducts(int count)
    {
        var products = new List<Product>();
        for (int i = 1; i <= count; i++)
        {
            var product = Product.Create(
                Guid.NewGuid(),
                $"Product {i}",
                $"Description for product {i}",
                100m + i,
                1m + i
            );
            products.Add(product);
        }
        return products;
    }
}