public class ComponentMetadata
{
    public string? Name { get; set; }
    public Dictionary<string, object> Parameters { get; set; } =
        new Dictionary<string, object>();
}
