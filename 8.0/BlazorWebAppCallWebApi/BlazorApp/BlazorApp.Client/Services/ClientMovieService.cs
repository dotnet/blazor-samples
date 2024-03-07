using Microsoft.AspNetCore.Components;
using BlazorApp.Client.Models;
using System.Net.Http.Json;

namespace BlazorApp.Client.Services;

public class ClientMovieService(HttpClient httpClient) : IMovieService
{
    private HttpClient Http { get; set; } = httpClient;

    public async Task<Movie[]> GetMoviesAsync(bool watchedMovies)
    {
        var requestUri = watchedMovies ? $"movies/watched" : "movies";
        return await Http.GetFromJsonAsync<Movie[]>(requestUri) ?? [];
    }

    public async Task PostMovieAsync(Movie movie)
    {
        if (Http is not null)
        {
            await Http.PostAsJsonAsync("movies", movie);
        }
    }

    public async Task PutMovieAsync(long id, Movie movie)
    {
        if (Http is not null)
        {
            await Http.PutAsJsonAsync($"movies/{id}", movie);
        }
    }

    public async Task DeleteMovieAsync(long id)
    {
        if (Http is not null)
        {
            await Http.DeleteAsync($"movies/{id}");
        }
    }
}
