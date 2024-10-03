using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using BlazorWebAppEFCore.Data;

namespace BlazorWebAppEFCore.Grid;

// Creates the correct expressions to filter and sort.
public class GridQueryAdapter
{
    // Holds state of the grid.
    private readonly IContactFilters controls;

    // Expressions for sorting.
    private readonly Dictionary<ContactFilterColumns, Expression<Func<Contact, string>>> expressions =
        new()
        {
            { ContactFilterColumns.City, c => c != null && c.City != null ? c.City : string.Empty },
            { ContactFilterColumns.Phone, c => c != null && c.Phone != null ? c.Phone : string.Empty },
            { ContactFilterColumns.Name, c => c != null && c.LastName != null ? c.LastName : string.Empty },
            { ContactFilterColumns.State, c => c != null && c.State != null ? c.State : string.Empty },
            { ContactFilterColumns.Street, c => c != null && c.Street != null ? c.Street : string.Empty },
            { ContactFilterColumns.ZipCode, c => c != null && c.ZipCode != null ? c.ZipCode : string.Empty }
        };

    // Queryables for filtering.
    private readonly Dictionary<ContactFilterColumns, Func<IQueryable<Contact>, IQueryable<Contact>>> filterQueries = [];

    // Creates a new instance of the GridQueryAdapter class.
    // controls: The IContactFilters" to use.
    public GridQueryAdapter(IContactFilters controls)
    {
        this.controls = controls;

        // Set up queries.
        filterQueries =
            new()
            {
                { ContactFilterColumns.City, cs => cs.Where(c => c != null && c.City != null && this.controls.FilterText != null && c.City.Contains(this.controls.FilterText) ) },
                { ContactFilterColumns.Phone, cs => cs.Where(c => c != null && c.Phone != null && this.controls.FilterText != null && c.Phone.Contains(this.controls.FilterText) ) },
                { ContactFilterColumns.Name, cs => cs.Where(c => c != null && c.FirstName != null && this.controls.FilterText != null && c.FirstName.Contains(this.controls.FilterText) ) },
                { ContactFilterColumns.State, cs => cs.Where(c => c != null && c.State != null && this.controls.FilterText != null && c.State.Contains(this.controls.FilterText) ) },
                { ContactFilterColumns.Street, cs => cs.Where(c => c != null && c.Street != null && this.controls.FilterText != null && c.Street.Contains(this.controls.FilterText) ) },
                { ContactFilterColumns.ZipCode, cs => cs.Where(c => c != null && c.ZipCode != null && this.controls.FilterText != null && c.ZipCode.Contains(this.controls.FilterText) ) }
            };
    }

    // Uses the query to return a count and a page.
    // query: The IQueryable{Contact} to work from.
    // Returns the resulting ICollection{Contact}.
    public async Task<ICollection<Contact>> FetchAsync(IQueryable<Contact> query)
    {
        query = FilterAndQuery(query);
        await CountAsync(query);
        var collection = await FetchPageQuery(query).ToListAsync();
        controls.PageHelper.PageItems = collection.Count;
        return collection;
    }

    // Get total filtered items count.
    // query: The IQueryable{Contact} to use.
    public async Task CountAsync(IQueryable<Contact> query) =>
        controls.PageHelper.TotalItemCount = await query.CountAsync();

    // Build the query to bring back a single page.
    // query: The <see IQueryable{Contact} to modify.
    // Returns the new IQueryable{Contact} for a page.
    public IQueryable<Contact> FetchPageQuery(IQueryable<Contact> query) =>
        query
            .Skip(controls.PageHelper.Skip)
            .Take(controls.PageHelper.PageSize)
            .AsNoTracking();

    // Builds the query.
    // root: The IQueryable{Contact} to start with.
    // Returns the resulting IQueryable{Contact} with sorts and filters applied.
    private IQueryable<Contact> FilterAndQuery(IQueryable<Contact> root)
    {
        var sb = new System.Text.StringBuilder();

        // Apply a filter?
        if (!string.IsNullOrWhiteSpace(controls.FilterText))
        {
            var filter = filterQueries[controls.FilterColumn];
            sb.Append($"Filter: '{controls.FilterColumn}' ");
            root = filter(root);
        }

        // Apply the expression.
        var expression = expressions[controls.SortColumn];
        sb.Append($"Sort: '{controls.SortColumn}' ");

        // Fix name.
        if (controls.SortColumn == ContactFilterColumns.Name && controls.ShowFirstNameFirst)
        {
            sb.Append("(first name first) ");
            expression = c => c.FirstName ?? string.Empty;
        }

        var sortDir = controls.SortAscending ? "ASC" : "DESC";
        sb.Append(sortDir);

        Debug.WriteLine(sb.ToString());

        // Return the unfiltered query for total count, and the filtered for fetching.
        return controls.SortAscending ? root.OrderBy(expression) : root.OrderByDescending(expression);
    }
}
