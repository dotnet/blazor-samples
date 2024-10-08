using Microsoft.JSInterop;

namespace BlazorSample;

public class HelloHelper(string? name)
{
    public string? Name { get; set; } = name ?? "No Name";

    [JSInvokable]
    public string GetHelloMessage() => $"Hello, {Name}!";
}
