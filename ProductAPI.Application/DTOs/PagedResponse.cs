namespace ProductAPI.Application.DTOs;

/// <summary>
/// Paginated response container
/// </summary>
/// <typeparam name="T">Type of items in the collection</typeparam>
public record PagedResponse<T>(
    /// <summary>
    /// Collection of items for current page
    /// </summary>
    IEnumerable<T> Items,
    
    /// <summary>
    /// Current page number
    /// </summary>
    /// <example>1</example>
    int PageNumber,
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    /// <example>10</example>
    int PageSize,
    
    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    /// <example>25</example>
    int TotalCount,
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    /// <example>3</example>
    int TotalPages,
    
    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    /// <example>false</example>
    bool HasPreviousPage,
    
    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    /// <example>true</example>
    bool HasNextPage
)
{
    /// <summary>
    /// Creates a paged response from a collection and pagination parameters
    /// </summary>
    public static PagedResponse<T> Create(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var hasPreviousPage = pageNumber > 1;
        var hasNextPage = pageNumber < totalPages;

        return new PagedResponse<T>(
            items,
            pageNumber,
            pageSize,
            totalCount,
            totalPages,
            hasPreviousPage,
            hasNextPage
        );
    }
};