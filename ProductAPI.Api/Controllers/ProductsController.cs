using Microsoft.AspNetCore.Mvc;
using ProductAPI.Application.DTOs;
using ProductAPI.Application.UseCases;
using ProductAPI.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace ProductAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly CreateProductUseCase _createProductUseCase;
    private readonly GetAllProductsUseCase _getAllProductsUseCase;

    public ProductsController(CreateProductUseCase createProductUseCase, GetAllProductsUseCase getAllProductsUseCase)
    {
        _createProductUseCase = createProductUseCase;
        _getAllProductsUseCase = getAllProductsUseCase;
    }

    /// <summary>
    /// Gets all products with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (starting from 1)</param>
    /// <param name="pageSize">Number of items per page (max 100)</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="400">Invalid pagination parameters</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAllProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var paginationRequest = new PaginationRequest(pageNumber, pageSize);
        var result = await _getAllProductsUseCase.ExecuteAsync(paginationRequest);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="request">Product creation request</param>
    /// <returns>Created product</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid request data or business rule violation</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            var product = await _createProductUseCase.ExecuteAsync(request);
            return CreatedAtAction(nameof(CreateProduct), new { id = product.Id }, product);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}