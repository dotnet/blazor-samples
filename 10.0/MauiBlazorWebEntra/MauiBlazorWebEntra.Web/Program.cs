using MauiBlazorWebEntra.Shared.Services;
using MauiBlazorWebEntra.Web.Components;
using MauiBlazorWebEntra.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddCascadingAuthenticationState();

var authentication = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
});

var entraConfiguration = builder.Configuration.GetRequiredSection("EntraExternalId"); // Or "EntraWorkforceId".
authentication.AddMicrosoftIdentityWebApp(entraConfiguration);
authentication.AddMicrosoftIdentityWebApi(entraConfiguration);

// A cookie issued for one tenant profile must not authenticate the other.
builder.Services.AddDataProtection()
    .SetApplicationName($"MauiBlazorWebEntra.{entraConfiguration.Key}");
builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme,
    options => options.Cookie.Name = $"MauiBlazorWebEntra.{entraConfiguration.Key}");

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.ResponseType = "code";
    options.MapInboundClaims = false;
    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "roles";
    options.TokenValidationParameters.AuthenticationType = CookieAuthenticationDefaults.AuthenticationScheme;
});

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters.AuthenticationType = JwtBearerDefaults.AuthenticationScheme;
});

// An explicit bearer header must never fall back to a browser cookie.
authentication.AddPolicyScheme("WeatherApi", null, options =>
{
    options.ForwardDefaultSelector = context =>
        context.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? JwtBearerDefaults.AuthenticationScheme
            : CookieAuthenticationDefaults.AuthenticationScheme;
    options.ForwardChallenge = JwtBearerDefaults.AuthenticationScheme;
    options.ForwardForbid = JwtBearerDefaults.AuthenticationScheme;
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("WeatherApi", policy => policy
        .AddAuthenticationSchemes("WeatherApi")
        .RequireAuthenticatedUser()
        .RequireAssertion(context => context.User.Identities.Any(identity =>
            identity.IsAuthenticated &&
            (identity.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme ||
             (identity.AuthenticationType == JwtBearerDefaults.AuthenticationScheme &&
              identity.FindAll("scp")
                  .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                  .Contains("Weather.Read", StringComparer.Ordinal))))));

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

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MauiBlazorWebEntra.Web.Client._Imports).Assembly,
        typeof(MauiBlazorWebEntra.Shared._Imports).Assembly);

app.MapGet("/authentication/login", (string? returnUrl) =>
{
    if (returnUrl is not null && !RedirectHttpResult.IsLocalUrl(returnUrl))
    {
        return Results.BadRequest("The return URL must be local.");
    }

    return Results.Challenge(new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
        [OpenIdConnectDefaults.AuthenticationScheme]);
}).AllowAnonymous();

// Binding a form value also enables antiforgery validation for this endpoint.
app.MapPost("/authentication/logout", ([FromForm] string? returnUrl) =>
{
    if (returnUrl is not null && !RedirectHttpResult.IsLocalUrl(returnUrl))
    {
        return Results.BadRequest("The return URL must be local.");
    }

    return Results.SignOut(new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
        [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
});

app.MapGet("/api/weather", async (IWeatherService weatherService) =>
    TypedResults.Ok(await weatherService.GetWeatherForecastsAsync()))
    .RequireAuthorization("WeatherApi")
    .WithMetadata(new SkipStatusCodePagesAttribute());

app.Run();
