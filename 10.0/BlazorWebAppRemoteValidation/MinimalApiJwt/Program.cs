using MinimalApiJwt.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

// Enables Microsoft.Extensions.Validation so data annotations on StarshipModel are
// validated before the endpoint handler runs.
builder.Services.AddValidation();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Remote validation endpoint. Data annotations on StarshipModel are validated by the
// framework. This handler adds the business rule that a 'Defense' ship requires a
// description, which is only validated on the server.
app.MapPost("/api-starship-validation", (
    StarshipModel model, ILogger<Program> logger) =>
{
    Dictionary<string, string[]> errors = [];

    if (model.Classification == "Defense" && string.IsNullOrEmpty(model.Description))
    {
        errors.Add(nameof(model.Description),
            ["For a 'Defense' ship, 'Description' is required."]);
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(
            errors: errors,
            detail: "One or more validation errors occurred.",
            instance: typeof(Program).Assembly.GetName().Name,
            title: "Validation Errors",
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1");
    }

    return Results.NoContent();

}).RequireAuthorization();

app.Run();
