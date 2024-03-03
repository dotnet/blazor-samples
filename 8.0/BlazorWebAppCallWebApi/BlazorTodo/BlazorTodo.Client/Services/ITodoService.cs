using BlazorTodo.Client.Models;

namespace BlazorTodo.Client.Services;

public interface ITodoService
{
    public Task<TodoItem[]> GetTodosAsync(bool completedItems);

    public Task PostTodoAsync(TodoItem todo);

    public Task PutTodoAsync(long id, TodoItem inputTodo);

    public Task DeleteTodoAsync(long id);
}
