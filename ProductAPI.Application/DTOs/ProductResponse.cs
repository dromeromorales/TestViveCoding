namespace ProductAPI.Application.DTOs;

/// <summary>
/// Product response model
/// </summary>
public record ProductResponse(
    /// <summary>
    /// Product unique identifier
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    Guid Id,
    
    /// <summary>
    /// Product name
    /// </summary>
    /// <example>Laptop HP Pavilion</example>
    string Name,
    
    /// <summary>
    /// Product description
    /// </summary>
    /// <example>High-performance laptop with 16GB RAM and SSD</example>
    string Description,
    
    /// <summary>
    /// Product price in USD
    /// </summary>
    /// <example>899.99</example>
    decimal Price,
    
    /// <summary>
    /// Product weight in kg
    /// </summary>
    /// <example>2.5</example>
    decimal Weight
);