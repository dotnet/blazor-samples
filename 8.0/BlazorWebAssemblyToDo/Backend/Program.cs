using Microsoft.EntityFrameworkCore;
using Backend;
using Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add the database (in memory for the sample)
builder.Services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));

// Add a CORS policy for the client
// Add .AllowCredentials() for apps that use an Identity Provider for authn/z
builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.WithOrigins([builder.Configuration["BackendUrl"] ?? "https://localhost:5001", 
            builder.Configuration["FrontendUrl"] ?? "https://localhost:5002"])
            .AllowAnyMethod()
            .AllowAnyHeader()));

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Add NSwag services
builder.Services.AddOpenApiDocument();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    // Seed the database
    await using var scope = app.Services.CreateAsyncScope();
    await SeedData.InitializeAsync(scope.ServiceProvider);

    // Add OpenAPI/Swagger generator and the Swagger UI
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// Activate the CORS policy
app.UseCors("wasm");

app.UseHttpsRedirection();

// Set up API endpoints and methods
var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", GetAllTodos);
todoItems.MapGet("/complete", GetCompleteTodos);
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);

app.Run();

static async Task<IResult> GetAllTodos(TodoContext db)
{
    return TypedResults.Ok(await db.TodoItems.ToArrayAsync());
}

static async Task<IResult> GetCompleteTodos(TodoContext db)
{
    return TypedResults.Ok(await db.TodoItems.Where(t => t.IsComplete).ToListAsync());
}

static async Task<IResult> GetTodo(long id, TodoContext db)
{
    return await db.TodoItems.FindAsync(id) is TodoItem todo ? TypedResults.Ok(todo) : TypedResults.NotFound();
}

static async Task<IResult> CreateTodo(TodoItem todo, TodoContext db)
{
    db.TodoItems.Add(todo);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/todoitems/{todo.Id}", todo);
}

static async Task<IResult> UpdateTodo(long id, TodoItem inputTodo, TodoContext db)
{
    var todo = await db.TodoItems.FindAsync(id);

    if (todo is null)
    {
        return TypedResults.NotFound();
    }

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodo(long id, TodoContext db)
{
    if (await db.TodoItems.FindAsync(id) is TodoItem todo)
    {
        db.TodoItems.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}
