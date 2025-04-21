var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", jwtOptions =>
    {
        // {TENANT ID} is the directory (tenant) ID.
        //
        // Authority format {AUTHORITY} matches the issurer (`iss`) of the JWT returned by the identity provider.
        //
        // Authority format {AUTHORITY} for ME-ID tenant type: https://sts.windows.net/{TENANT ID}/
        // Authority format {AUTHORITY} for B2C tenant type: https://login.microsoftonline.com/{TENANT ID}/v2.0/
        //
        jwtOptions.Authority = "{AUTHORITY}";
        //
        // The following should match just the path of the Application ID URI configured when adding the "Weather.Get" scope
        // under "Expose an API" in the Azure or Entra portal. {CLIENT ID} is the application (client) ID of this 
        // app's registration in the Azure portal.
        // 
        // Audience format {AUDIENCE} for ME-ID tenant type: api://{CLIENT ID}
        // Audience format {AUDIENCE} for B2C tenant type: https://{DIRECTORY NAME}.onmicrosoft.com/{CLIENT ID}
        //
        jwtOptions.Audience = "{AUDIENCE}";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Secure web API for weather forecast data
// The following MapGet call chains a call to
// RequireAuthorization. This endpoint only
// returns data when a user is signed into the
// app.
app.MapGet("/weather-forecast-secure", () =>
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

// Insecure web API for weather forebase data
// The following MapGet call doesn't chain a call to
// RequireAuthorization. This endpoint returns data
// when a user isn't signed into the app.
app.MapGet("/weather-forecast-insecure", () =>
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
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
