namespace BlazorApp.Client.Models;

public sealed class Movie
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public bool IsWatched { get; set; }
}
