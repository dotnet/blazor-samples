using BlazorSample.Client.Starship;
using BlazorSample.Components;
using BlazorSample.Starship;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// Register the server form validator for the IFormValidation interface. The server form
// validator is used on the server to call ValidateStarshipFormAsync for form validation.
builder.Services.AddScoped<IFormValidation, ServerFormValidation>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

// Server-side endpoint that proxies form validation for components rendered on the server.
app.MapPost("/starship-validation", (IFormValidation formValidator,
    StarshipModel model) =>
{
    return formValidator.ValidateStarshipFormAsync(model);
}).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorSample.Client._Imports).Assembly);

app.Run();
