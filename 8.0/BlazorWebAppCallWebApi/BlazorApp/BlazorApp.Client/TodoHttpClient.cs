using System.Net.Http.Json;
using BlazorApp.Client.Models;

namespace BlazorApp.Client;

public class TodoHttpClient(IConfiguration config, HttpClient http)
{
    public async Task<TodoItem[]> GetTodoItemsAsync()
    {
        return await http.GetFromJsonAsync<TodoItem[]>($"{config.GetValue<string>("BackendUrl")}/todoitems") ?? [];
    }
}
