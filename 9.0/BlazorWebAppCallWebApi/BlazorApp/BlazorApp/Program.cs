using BlazorApp.Client.Pages;
using BlazorApp.Components;
using Microsoft.EntityFrameworkCore;
using BlazorApp;
using BlazorApp.Models;
using BlazorApp.Client.Models;
using BlazorApp.Client.Services;
using BlazorApp.Services;
using Microsoft.AspNetCore.Hosting;
using BlazorApp.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add HttpClient for making external web API calls to the Backend server API
builder.Services.AddHttpClient();

// For prerendering purposes, register a named HttpClient for the app's
// named client component example.
builder.Services.AddHttpClient("WebAPI", client =>
    client.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001"));

// For prerendering purposes, register the client app's typed HttpClient
// for the app's typed client component example.
builder.Services.AddHttpClient<TodoHttpClient>(client => client.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5002"));

// Add Todo service for components adopting SSR
builder.Services.AddScoped<IMovieService, ServerMovieService>();

// Add the database (in memory for the sample)
builder.Services.AddDbContext<MovieContext>(opt => opt.UseInMemoryDatabase("TodoList"));

// Add a CORS policy for the client
// Add .AllowCredentials() for apps that use an Identity Provider for authn/z
builder.Services.AddCors(
    options => options.AddPolicy(
        "server",
        policy => policy.WithOrigins(builder.Configuration["FrontendUrl"] ?? "https://localhost:5002")
            .AllowAnyMethod()
            .AllowAnyHeader()));

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Add NSwag services
builder.Services.AddOpenApiDocument();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();

    // Seed the database
    await using var scope = app.Services.CreateAsyncScope();
    await SeedData.InitializeAsync(scope.ServiceProvider);

    // Add OpenAPI/Swagger generator and the Swagger UI
    app.UseOpenApi();
    app.UseSwaggerUi();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Activate the CORS policy
app.UseCors("server");

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();

// Set up API endpoints and methods
var todoItems = app.MapGroup("/movies");

todoItems.MapGet("/", GetAllMovies);
todoItems.MapGet("/watched", GetWatchedMovies);
todoItems.MapGet("/{id}", GetMovie);
todoItems.MapPost("/", CreateMovie);
todoItems.MapPut("/{id}", UpdateMovie);
todoItems.MapDelete("/{id}", DeleteMovie);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorApp.Client._Imports).Assembly);

app.Run();

static async Task<IResult> GetAllMovies(MovieContext db)
{
    return TypedResults.Ok(await db.Movies.ToArrayAsync());
}

static async Task<IResult> GetWatchedMovies(MovieContext db)
{
    return TypedResults.Ok(await db.Movies.Where(t => t.IsWatched).ToListAsync());
}

static async Task<IResult> GetMovie(long id, MovieContext db)
{
    return await db.Movies.FindAsync(id) is Movie movie ? TypedResults.Ok(movie) : TypedResults.NotFound();
}

static async Task<IResult> CreateMovie(Movie movie, MovieContext db)
{
    db.Movies.Add(movie);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/movies/{movie.Id}", movie);
}

static async Task<IResult> UpdateMovie(long id, Movie inputMovie, MovieContext db)
{
    var movie = await db.Movies.FindAsync(id);

    if (movie is null)
    {
        return TypedResults.NotFound();
    }

    movie.Name = inputMovie.Name;
    movie.IsWatched = inputMovie.IsWatched;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteMovie(long id, MovieContext db)
{
    if (await db.Movies.FindAsync(id) is Movie movie)
    {
        db.Movies.Remove(movie);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}
