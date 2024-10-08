namespace BlazorWebAppEFCore.Grid;

public interface IPageHelper
{
    // Current page, 0-based.
    int DbPage { get; }

    // Current page, 1-based.
    int Page { get; set; }

    // Previous page, 1-based.
    int PrevPage { get; }

    // Next page, 1-based.
    int NextPage { get; }

    // True when a previous page exists.
    bool HasPrev { get; }

    // True when a next page exists.
    bool HasNext { get; }

    // Total page count.
    int PageCount { get; }

    // Items on current page.
    int PageItems { get; set; }

    // Items per page.
    int PageSize { get; set; }

    // How many items to skip.
    int Skip { get; }

    // Total items based on filter.
    int TotalItemCount { get; set; }
}
