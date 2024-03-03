using Microsoft.AspNetCore.Components;
using BlazorTodo.Client.Models;
using System.Net.Http.Json;

namespace BlazorTodo.Client.Services;

public class ClientTodoService(IConfiguration config) : ITodoService
{
    private readonly string serviceEndpoint = $"{config.GetValue<string>("FrontendUrl")}/todoitems";

    [Inject]
    private HttpClient? Http { get; set; }

    public async Task<TodoItem[]> GetTodosAsync(bool completedItems)
    {
        var requestUri = completedItems ? $"{serviceEndpoint}/complete" : serviceEndpoint;
        return Http is not null ? await Http.GetFromJsonAsync<TodoItem[]>(requestUri) ?? [] : [];
    }

    public async Task PostTodoAsync(TodoItem todo)
    {
        if (Http is not null)
        {
            await Http.PostAsJsonAsync(serviceEndpoint, todo);
        }
    }

    public async Task PutTodoAsync(long id, TodoItem inputTodo)
    {
        if (Http is not null)
        {
            await Http.PutAsJsonAsync($"{serviceEndpoint}/{id}", inputTodo);
        }
    }

    public async Task DeleteTodoAsync(long id)
    {
        if (Http is not null)
        {
            await Http.DeleteAsync($"{serviceEndpoint}/{id}");
        }
    }
}
