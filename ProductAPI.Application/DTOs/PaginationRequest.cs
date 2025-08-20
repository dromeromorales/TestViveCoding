using System.ComponentModel.DataAnnotations;

namespace ProductAPI.Application.DTOs;

/// <summary>
/// Pagination request parameters
/// </summary>
public record PaginationRequest(
    /// <summary>
    /// Page number (starting from 1)
    /// </summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    int PageNumber = 1,
    
    /// <summary>
    /// Number of items per page (maximum 100)
    /// </summary>
    /// <example>10</example>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    int PageSize = 10
);