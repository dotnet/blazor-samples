using Microsoft.EntityFrameworkCore;
using BlazorWebAppMovies.Models;
using Humanizer;
using System;

namespace BlazorWebAppMovies.Data
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new BlazorWebAppMoviesContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<BlazorWebAppMoviesContext>>());

            if (context == null || context.Movie == null)
            {
                throw new NullReferenceException(
                    "Null BlazorWebAppMoviesContext or Movie DbSet");
            }

            if (context.Movie.Any())
            {
                return;
            }

            /* The following is used up to Part 7 of
            the tutorial series.

            context.Movie.AddRange(
                new Movie
                {
                    Title = "Mad Max",
                    ReleaseDate = DateTime.Parse("1979-4-12"),
                    Genre = "Sci-fi (Cyberpunk)",
                    Price = 2.51M,
                },
                new Movie
                {
                    Title = "The Road Warrior",
                    ReleaseDate = DateTime.Parse("1981-12-24"),
                    Genre = "Sci-fi (Cyberpunk)",
                    Price = 2.78M,
                },
                new Movie
                {
                    Title = "Mad Max: Beyond Thunderdome",
                    ReleaseDate = DateTime.Parse("1985-7-10"),
                    Genre = "Sci-fi (Cyberpunk)",
                    Price = 3.55M,
                },
                new Movie
                {
                    Title = "Mad Max: Fury Road",
                    ReleaseDate = DateTime.Parse("2015-5-15"),
                    Genre = "Sci-fi (Cyberpunk)",
                    Price = 8.43M,
                },
                new Movie
                {
                    Title = "Furiosa: A Mad Max Saga",
                    ReleaseDate = DateTime.Parse("2024-5-24"),
                    Genre = "Sci-fi (Cyberpunk)",
                    Price = 13.49M,
                }
            );

            */
            /* The following is used in Parts 7 and 8, the
            last two parts of the tutorial series.
            */
            context.Movie.AddRange(
                new Movie
                {
                    Title = "Mad Max",
                    ReleaseDate = DateTime.Parse("1979-4-12"),
                    Genre = "Sci-fi (Cyberpunk)",
                    Price = 2.51M,
                    Rating = "R",
                },
                new Movie
                {
                    Title = "The Road Warrior",
                    ReleaseDate = DateTime.Parse("1981-12-24"),
                    Genre = "Sci-fi (Cyberpunk)",
                    Price = 2.78M,
                    Rating = "R",
                },
                new Movie
                {
                    Title = "Mad Max: Beyond Thunderdome",
                    ReleaseDate = DateTime.Parse("1985-7-10"),
                    Genre = "Sci-fi (Cyberpunk)",
                    Price = 3.55M,
                    Rating = "PG-13",
                },
                new Movie
                {
                    Title = "Mad Max: Fury Road",
                    ReleaseDate = DateTime.Parse("2015-5-15"),
                    Genre = "Sci-fi (Cyberpunk)",
                    Price = 8.43M,
                    Rating = "R",
                },
                new Movie
                {
                    Title = "Furiosa: A Mad Max Saga",
                    ReleaseDate = DateTime.Parse("2024-5-24"),
                    Genre = "Sci-fi (Cyberpunk)",
                    Price = 13.49M,
                    Rating = "R",
                }
            );

            context.SaveChanges();
        }
    }
}
