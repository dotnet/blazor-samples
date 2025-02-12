using Microsoft.AspNetCore.Authentication.Negotiate;
using BlazorWebAppWinAuthServer.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddCascadingAuthenticationState();

// https://learn.microsoft.com/windows-server/identity/ad-ds/manage/understand-security-identifiers
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("LocalAccount", policy =>
        policy.RequireClaim(
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid",
            "S-1-5-113"));   

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
