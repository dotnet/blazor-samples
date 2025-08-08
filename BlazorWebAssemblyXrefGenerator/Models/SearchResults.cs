namespace BlazorWebAssemblyXrefGenerator.Models
{
    public class SearchResults
    {
        public IEnumerable<SearchResult> Results { get; set; } = [];
    }

    public class SearchResult
    {
        public string? DisplayName { get; set; }
        public string? Url { get; set; }
        public string? ItemType { get; set; }
        public string? Description { get; set; }
        public string? Link { get; set; }
        public int Index { get; set; }
    }
}
