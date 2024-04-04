public class ComponentMetadata
{
    public Type? Type { get; init; }
    public string? Name { get; init; }
    public Dictionary<string, object> Parameters { get; } = new();
}
