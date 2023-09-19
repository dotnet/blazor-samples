namespace BlazorWebAppEFCore.Grid;

// State of grid filters.
public class GridControls : IContactFilters
{
    // Keep state of paging.
    public IPageHelper PageHelper { get; set; }

    public GridControls(IPageHelper pageHelper)
    {
        PageHelper = pageHelper;
    }

    // Avoid multiple concurrent requests.
    public bool Loading { get; set; }

    // Firstname Lastname, or Lastname, Firstname.
    public bool ShowFirstNameFirst { get; set; }

    // Column to sort by.
    public ContactFilterColumns SortColumn { get; set; }
        = ContactFilterColumns.Name;

    // True when sorting ascending, otherwise sort descending.
    public bool SortAscending { get; set; } = true;

    // Column filtered text is against.
    public ContactFilterColumns FilterColumn { get; set; } = ContactFilterColumns.Name;

    // Text to filter on.
    public string? FilterText { get; set; }
}
