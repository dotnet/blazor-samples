using BlazorWebAssemblyXrefGenerator.Models;

namespace BlazorWebAssemblyXrefGenerator.Services.Interfaces
{
    public interface ISearchService
    {
        Task<IEnumerable<SearchResult>> GetSearchResultsAsync(string searchText);

        Task<SearchResult> GetSearchResultDataAsync(SearchResult searchResult, string? dotNetVersion);
    }
}
