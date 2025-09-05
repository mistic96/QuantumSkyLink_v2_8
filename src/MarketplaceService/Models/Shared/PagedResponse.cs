namespace MarketplaceService.Models.Shared;

/// <summary>
/// Generic paginated response model
/// </summary>
/// <typeparam name="T">Type of data items</typeparam>
public class PagedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Generic paginated listing response
/// </summary>
/// <typeparam name="T">Type of listing items</typeparam>
public class PaginatedListingResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
