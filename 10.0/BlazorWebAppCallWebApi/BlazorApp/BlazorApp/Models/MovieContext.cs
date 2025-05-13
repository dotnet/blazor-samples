using Microsoft.EntityFrameworkCore;
using BlazorApp.Client.Models;

namespace BlazorApp.Models;

public class MovieContext(DbContextOptions<MovieContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies { get; set; } = null!;
}
