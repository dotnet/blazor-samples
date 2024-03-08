using System.Net.Http.Json;
using BlazorApp.Client.Models;

namespace BlazorApp.Client;

public class TodoHttpClient(HttpClient client)
{
    public async Task<TodoItem[]> GetTodoItemsAsync()
    {
        return await client.GetFromJsonAsync<TodoItem[]>("todoitems") ?? [];
    }
}
