namespace Invoice.Application.DTOs;

/// <summary>
/// A page of results along with the total count across all pages.
/// </summary>
public class PagedResult<T>
{
    /// <summary>The items in this page.</summary>
    public required List<T> Items { get; set; }

    /// <summary>The total number of items across all pages.</summary>
    public int TotalCount { get; set; }

    /// <summary>The current page number (1-based).</summary>
    public int PageNumber { get; set; }

    /// <summary>The page size.</summary>
    public int PageSize { get; set; }
}
