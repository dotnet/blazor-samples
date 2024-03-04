using Microsoft.AspNetCore.Components;
using BlazorApp.Client.Models;
using System.Net.Http.Json;

namespace BlazorApp.Client.Services;

public class ClientMovieService(IConfiguration config, HttpClient httpClient) : IMovieService
{
    private readonly string serviceEndpoint = $"{config.GetValue<string>("FrontendUrl")}/movies";

    private HttpClient Http { get; set; } = httpClient;

    public async Task<Movie[]> GetMoviesAsync(bool watchedMovies)
    {
        var requestUri = watchedMovies ? $"{serviceEndpoint}/watched" : serviceEndpoint;
        return await Http.GetFromJsonAsync<Movie[]>(requestUri) ?? [];
    }

    public async Task PostMovieAsync(Movie movie)
    {
        if (Http is not null)
        {
            await Http.PostAsJsonAsync(serviceEndpoint, movie);
        }
    }

    public async Task PutMovieAsync(long id, Movie movie)
    {
        if (Http is not null)
        {
            await Http.PutAsJsonAsync($"{serviceEndpoint}/{id}", movie);
        }
    }

    public async Task DeleteMovieAsync(long id)
    {
        if (Http is not null)
        {
            await Http.DeleteAsync($"{serviceEndpoint}/{id}");
        }
    }
}
