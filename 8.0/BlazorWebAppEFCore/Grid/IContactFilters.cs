namespace BlazorWebAppEFCore.Grid;

// Interface for filtering.
public interface IContactFilters
{
    // The ContactFilterColumns being filtered on.
    ContactFilterColumns FilterColumn { get; set; }

    // Loading indicator.
    bool Loading { get; set; }

    // The text of the filter.
    string? FilterText { get; set; }

    // Paging state in PageHelper.
    IPageHelper PageHelper { get; set; }

    // Gets or sets a value indicating if the name is rendered first name first.
    bool ShowFirstNameFirst { get; set; }

    // Gets or sets a value indicating if the sort is ascending or descending.
    bool SortAscending { get; set; }

    // The ContactFilterColumns being sorted.
    ContactFilterColumns SortColumn { get; set; }
}
