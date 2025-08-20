using Moq;
using ProductAPI.Application.DTOs;
using ProductAPI.Application.UseCases;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Exceptions;
using ProductAPI.Domain.Repositories;

namespace ProductAPI.Tests.UseCases;

public class CreateProductUseCaseTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly CreateProductUseCase _useCase;

    public CreateProductUseCaseTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _useCase = new CreateProductUseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldCreateProduct()
    {
        var request = new CreateProductRequest("Test Product", "Test Description", 100m, 5m);
        
        _mockRepository.Setup(x => x.SaveAsync(It.IsAny<Product>()))
                      .ReturnsAsync((Product product) => product);

        var result = await _useCase.ExecuteAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(100m, result.Price);
        Assert.Equal(5m, result.Weight);
        
        _mockRepository.Verify(x => x.SaveAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithPriceExceeding10000_ShouldThrowInvalidPriceException()
    {
        var request = new CreateProductRequest("Test Product", "Test Description", 10001m, 5m);

        var exception = await Assert.ThrowsAsync<InvalidPriceException>(
            () => _useCase.ExecuteAsync(request));
        
        Assert.Equal("Price cannot exceed $10000", exception.Message);
        _mockRepository.Verify(x => x.SaveAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithWeightExceeding80kg_ShouldThrowInvalidWeightException()
    {
        var request = new CreateProductRequest("Test Product", "Test Description", 100m, 81m);

        var exception = await Assert.ThrowsAsync<InvalidWeightException>(
            () => _useCase.ExecuteAsync(request));
        
        Assert.Equal("Weight cannot exceed 80kg", exception.Message);
        _mockRepository.Verify(x => x.SaveAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativePrice_ShouldThrowInvalidPriceException()
    {
        var request = new CreateProductRequest("Test Product", "Test Description", -1m, 5m);

        var exception = await Assert.ThrowsAsync<InvalidPriceException>(
            () => _useCase.ExecuteAsync(request));
        
        Assert.Equal("Price cannot be negative", exception.Message);
        _mockRepository.Verify(x => x.SaveAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeWeight_ShouldThrowInvalidWeightException()
    {
        var request = new CreateProductRequest("Test Product", "Test Description", 100m, -1m);

        var exception = await Assert.ThrowsAsync<InvalidWeightException>(
            () => _useCase.ExecuteAsync(request));
        
        Assert.Equal("Weight cannot be negative", exception.Message);
        _mockRepository.Verify(x => x.SaveAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyName_ShouldThrowArgumentException()
    {
        var request = new CreateProductRequest("", "Test Description", 100m, 5m);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _useCase.ExecuteAsync(request));
        
        Assert.Equal("Name is required", exception.Message);
        _mockRepository.Verify(x => x.SaveAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyDescription_ShouldThrowArgumentException()
    {
        var request = new CreateProductRequest("Test Product", "", 100m, 5m);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _useCase.ExecuteAsync(request));
        
        Assert.Equal("Description is required", exception.Message);
        _mockRepository.Verify(x => x.SaveAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithMaxAllowedValues_ShouldCreateProduct()
    {
        var request = new CreateProductRequest("Test Product", "Test Description", 10000m, 80m);
        
        _mockRepository.Setup(x => x.SaveAsync(It.IsAny<Product>()))
                      .ReturnsAsync((Product product) => product);

        var result = await _useCase.ExecuteAsync(request);

        Assert.Equal(10000m, result.Price);
        Assert.Equal(80m, result.Weight);
        _mockRepository.Verify(x => x.SaveAsync(It.IsAny<Product>()), Times.Once);
    }
}