using BlazorApp.Client.Models;
using System.Net.Http.Json;

namespace BlazorApp.Client.Services;

public class ClientMovieService(HttpClient http) : IMovieService
{
    public async Task<Movie[]> GetMoviesAsync(bool watchedMovies)
    {
        var requestUri = watchedMovies ? $"movies/watched" : "movies";
        return await http.GetFromJsonAsync<Movie[]>(requestUri) ?? [];
    }

    public async Task PostMovieAsync(Movie movie)
    {
        await http.PostAsJsonAsync("movies", movie);
    }

    public async Task PutMovieAsync(long id, Movie movie)
    {
        await http.PutAsJsonAsync($"movies/{id}", movie);
    }

    public async Task DeleteMovieAsync(long id)
    {
        await http.DeleteAsync($"movies/{id}");
    }
}
