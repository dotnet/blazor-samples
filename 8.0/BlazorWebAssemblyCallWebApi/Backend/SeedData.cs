using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend;

public class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var context = new TodoContext(serviceProvider.GetRequiredService<DbContextOptions<TodoContext>>());

        context.Add(new TodoItem { Name = "Lift weights", IsComplete = true });
        context.Add(new TodoItem { Name = "Feed Merlin", IsComplete = false });
        context.Add(new TodoItem { Name = "Update web API article", IsComplete = true });
        context.Add(new TodoItem { Name = "Attend Blazor party", IsComplete = false });

        await context.SaveChangesAsync();
    }
}
