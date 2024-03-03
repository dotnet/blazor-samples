using Microsoft.EntityFrameworkCore;
using BlazorTodo.Client.Models;

namespace BlazorTodo.Models;

public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems { get; set; } = null!;
}
