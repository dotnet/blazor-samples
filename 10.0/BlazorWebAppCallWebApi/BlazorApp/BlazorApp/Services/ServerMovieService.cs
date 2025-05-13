using BlazorApp.Client.Models;
using BlazorApp.Client.Services;
using BlazorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class ServerMovieService(MovieContext db) : IMovieService
{
    public async Task<Movie[]> GetMoviesAsync(bool watchedMovies)
    {
        return watchedMovies ? 
            await db.Movies.Where(t => t.IsWatched).ToArrayAsync() : 
            await db.Movies.ToArrayAsync();
    }

    public async Task PostMovieAsync(Movie movie)
    {
        db.Movies.Add(movie);
        await db.SaveChangesAsync();
    }

    public async Task PutMovieAsync(long id, Movie inputMovie)
    {
        var movie = await db.Movies.FindAsync(id);

        if (movie is null)
        {
            return;
        }

        movie.Name = inputMovie.Name;
        movie.IsWatched = inputMovie.IsWatched;

        await db.SaveChangesAsync();
    }

    public async Task DeleteMovieAsync(long id)
    {
        if (await db.Movies.FindAsync(id) is Movie movie)
        {
            db.Movies.Remove(movie);
            await db.SaveChangesAsync();
        }
    }
}
