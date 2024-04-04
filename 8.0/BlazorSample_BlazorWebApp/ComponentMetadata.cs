namespace BlazorSample;

public class ComponentMetadata
{
    public required Type Type { get; init; }
    public required string Name { get; init; }
    public Dictionary<string, object> Parameters { get; } = [];
}
