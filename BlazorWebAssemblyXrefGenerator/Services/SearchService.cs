using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using BlazorWebAssemblyXrefGenerator.Models;
using BlazorWebAssemblyXrefGenerator.Services.Interfaces;

namespace BlazorWebAssemblyXrefGenerator.Services
{
    public class SearchService : ISearchService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _apiClient;
        private readonly HttpClient _proxyClient;

        public SearchService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _apiClient = _httpClientFactory.CreateClient("APIClient");
            _proxyClient = _httpClientFactory.CreateClient();
        }

        public async Task<IEnumerable<SearchResult>> GetSearchResultsAsync(string searchText)
        {
            var response = await _apiClient.GetFromJsonAsync<SearchResults>($"api/apibrowser/dotnet/search?api-version=0.2&search={searchText}");

            var searchResults = response?.Results;

            if (searchResults == null || searchResults.Any() == false)
            {
                return [];
            }

            return searchResults;
        }

        public async Task<SearchResult> GetSearchResultDataAsync(SearchResult searchResult, string? dotNetVersion)
        {
            var encodedUrl = WebUtility.UrlEncode($"https://learn.microsoft.com/en-us{searchResult.Url}?view={dotNetVersion}");
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://corsproxy.io/?{encodedUrl}");
            using var response = await _proxyClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            var match = Regex.Match(content, "<meta name=\"ms.assetid\" content=\"(.+?)\" />");
            if (match.Success)
            {
                searchResult.Link = match.Groups[1].Value.Replace("*", "%2A").Replace("`", "%60");
            }

            return searchResult;
        }
    }
}
