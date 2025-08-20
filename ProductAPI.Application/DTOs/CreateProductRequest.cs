using System.ComponentModel.DataAnnotations;

namespace ProductAPI.Application.DTOs;

/// <summary>
/// Request model for creating a new product
/// </summary>
public record CreateProductRequest(
    /// <summary>
    /// Product name
    /// </summary>
    /// <example>Laptop HP Pavilion</example>
    [Required]
    string Name,
    
    /// <summary>
    /// Product description
    /// </summary>
    /// <example>High-performance laptop with 16GB RAM and SSD</example>
    [Required]
    string Description,
    
    /// <summary>
    /// Product price in USD (maximum $10,000)
    /// </summary>
    /// <example>899.99</example>
    [Range(0, 10000)]
    decimal Price,
    
    /// <summary>
    /// Product weight in kg (maximum 80kg)
    /// </summary>
    /// <example>2.5</example>
    [Range(0, 80)]
    decimal Weight
);