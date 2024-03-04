using BlazorApp.Client.Models;

namespace BlazorApp.Client.Services;

public interface IMovieService
{
    public Task<Movie[]> GetMoviesAsync(bool watchedMovies);

    public Task PostMovieAsync(Movie movie);

    public Task PutMovieAsync(long id, Movie movie);

    public Task DeleteMovieAsync(long id);
}
