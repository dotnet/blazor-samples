using BlazorWebAppRemoteValidation.Components;
using BlazorWebAppRemoteValidation.Client.Models;
using BlazorWebAppRemoteValidation.Client.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddClientModelValidation();
builder.Services.AddValidation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.MapStaticAssets();

// <snippet_ValidationEndpoint>
app.MapPost("/api/starships/validate", (StarshipModel model) =>
{
    Dictionary<string, string[]> errors = [];

    if (model.Classification == "Defense" &&
        string.IsNullOrWhiteSpace(model.Description))
    {
        errors[nameof(model.Description)] =
            ["A defense ship requires a description."];
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    return Results.NoContent();
});
// </snippet_ValidationEndpoint>

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebAppRemoteValidation.Client._Imports).Assembly);

app.Run();
