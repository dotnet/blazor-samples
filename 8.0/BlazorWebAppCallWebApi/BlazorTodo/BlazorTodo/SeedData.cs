using Microsoft.EntityFrameworkCore;
using BlazorTodo.Models;
using BlazorTodo.Client.Models;

namespace BlazorTodo;

public class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var context = new TodoContext(serviceProvider.GetRequiredService<DbContextOptions<TodoContext>>());

        context.Add(new TodoItem { Name = "Internal: Lift weights", IsComplete = true });
        context.Add(new TodoItem { Name = "Internal: Feed Merlin", IsComplete = false });
        context.Add(new TodoItem { Name = "Internal: Update web API article", IsComplete = true });
        context.Add(new TodoItem { Name = "Internal: Attend Blazor party", IsComplete = false });

        await context.SaveChangesAsync();
    }
}
