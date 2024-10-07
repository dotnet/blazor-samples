namespace BlazorSample
{
    public class Ship
    {
        public string? Id { get; set; }
        public ShipDetails Details { get; set; } = new();
    }
}
