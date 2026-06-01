using MauiBlazorWebEntraWorkforce.Shared.Services;
using MauiBlazorWebEntraWorkforce.Web.Components;
using MauiBlazorWebEntraWorkforce.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add device-specific services used by the MauiBlazorWebEntraWorkforce.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

builder.Services.AddCascadingAuthenticationState();

// Dual authentication: OIDC + Cookie for web browser, JWT Bearer for MAUI API calls.
// A policy scheme routes requests to the correct handler based on the Authorization header.
var authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "BearerOrCookie";
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddPolicyScheme("BearerOrCookie", "Bearer or Cookie", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
                return JwtBearerDefaults.AuthenticationScheme;
            return CookieAuthenticationDefaults.AuthenticationScheme;
        };
    });

// OpenID Connect + Cookie for web browser users
authBuilder.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// JWT Bearer validation for MAUI client API calls
authBuilder.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"),
    jwtBearerScheme: JwtBearerDefaults.AuthenticationScheme);

builder.Services.AddAuthorization();

// For more information on OpenAPI support in ASP.NET Core,
// see OpenAPI support in ASP.NET Core API apps at
// https://learn.microsoft.com/aspnet/core/fundamentals/openapi/overview
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(MauiBlazorWebEntraWorkforce.Shared._Imports).Assembly);

// Login endpoint: triggers the OIDC redirect to the workforce tenant
app.MapGet("/authentication/login", async (HttpContext context, string? returnUrl) =>
{
    // Only allow local return URLs to prevent open-redirect attacks.
    if (string.IsNullOrEmpty(returnUrl) || !Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        returnUrl = "/";

    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = returnUrl });
});

// Logout endpoint: clears cookie and signs out of Entra
app.MapPost("/authentication/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/" });
});

// Add the weather API endpoint and require authorization
app.MapGet("/api/weather", async (IWeatherService weatherService) =>
{
    var forecasts = await weatherService.GetWeatherForecastsAsync();
    return Results.Ok(forecasts);
}).RequireAuthorization();

app.Run();
