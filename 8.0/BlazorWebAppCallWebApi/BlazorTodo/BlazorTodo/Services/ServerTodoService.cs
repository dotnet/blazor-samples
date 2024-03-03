using BlazorTodo.Client.Models;
using BlazorTodo.Client.Services;
using BlazorTodo.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorTodo.Services;

public class ServerTodoService(TodoContext db) : ITodoService
{
    public async Task<TodoItem[]> GetTodosAsync(bool completedItems)
    {
        return completedItems ? 
            await db.TodoItems.Where(t => t.IsComplete).ToArrayAsync() : 
            await db.TodoItems.ToArrayAsync();
    }

    public async Task PostTodoAsync(TodoItem todo)
    {
        db.TodoItems.Add(todo);
        await db.SaveChangesAsync();
    }

    public async Task PutTodoAsync(long id, TodoItem inputTodo)
    {
        var todo = await db.TodoItems.FindAsync(id);

        if (todo is null)
        {
            return;
        }

        todo.Name = inputTodo.Name;
        todo.IsComplete = inputTodo.IsComplete;

        await db.SaveChangesAsync();
    }

    public async Task DeleteTodoAsync(long id)
    {
        if (await db.TodoItems.FindAsync(id) is TodoItem todo)
        {
            db.TodoItems.Remove(todo);
            await db.SaveChangesAsync();
        }
    }
}
