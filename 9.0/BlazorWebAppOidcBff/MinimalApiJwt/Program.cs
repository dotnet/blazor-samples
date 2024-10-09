var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", jwtOptions =>
    {
        // The following should match the authority configured for the OIDC handler in BlazorWebAppOidc/Program.cs.
        // {TENANT ID} is the directory (tenant) ID. If using an ME-ID tenant type, the authority should match
        // the issurer (`iss`) of the JWT returned by the identity provider:
        // https://sts.windows.net/{TENANT ID}/
        jwtOptions.Authority = "https://login.microsoftonline.com/{TENANT ID}/v2.0/";
        // The following should match just the path of the Application ID URI configured when adding the "Weather.Get" scope
        // under "Expose an API" in the Azure or Entra portal. {CLIENT ID} is the application (client) ID of this 
        // app's registration in the Azure portal. If using an ME-ID tenant type, the format of the App ID URI is:
        // api://{CLIENT ID}
        jwtOptions.Audience = "https://{DIRECTORY NAME}.onmicrosoft.com/{CLIENT ID}";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapDefaultEndpoints();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weather-forecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
}).RequireAuthorization();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
