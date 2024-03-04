using Microsoft.EntityFrameworkCore;
using BlazorApp.Models;
using BlazorApp.Client.Models;

namespace BlazorApp;

public class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var context = new MovieContext(serviceProvider.GetRequiredService<DbContextOptions<MovieContext>>());

        context.Add(new Movie { Name = "Serenity (2005)", IsWatched = true });
        context.Add(new Movie { Name = "Star Trek II: The Wrath of Khan", IsWatched = false });
        context.Add(new Movie { Name = "The Man Who Fell to Earth", IsWatched = true });
        context.Add(new Movie { Name = "Starship Troopers", IsWatched = false });
        context.Add(new Movie { Name = "The Matrix", IsWatched = true });
        context.Add(new Movie { Name = "Star Wars", IsWatched = false });
        context.Add(new Movie { Name = "Metropolis", IsWatched = true });
        context.Add(new Movie { Name = "Alien", IsWatched = false });
        context.Add(new Movie { Name = "Blade Runner", IsWatched = true });
        context.Add(new Movie { Name = "Close Encounters of the Third Kind", IsWatched = false });
        context.Add(new Movie { Name = "2001: A Space Odyssey", IsWatched = true });

        await context.SaveChangesAsync();
    }
}
