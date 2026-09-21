namespace BlazorSample.Client.Starship;

public interface IFormValidation
{
    Task<IDictionary<string, string[]>> ValidateStarshipFormAsync(
        StarshipModel starship);
}
