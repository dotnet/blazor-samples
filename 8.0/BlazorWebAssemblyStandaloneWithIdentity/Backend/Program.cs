using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// cookie authentication
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();

// configure authorization
builder.Services.AddAuthorizationBuilder();

// add the database (in memory for the sample)
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("AppDb"));

// add identity and opt-in to endpoints
builder.Services.AddIdentityCore<AppUser>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

// add CORS policy for Wasm client
builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.WithOrigins([builder.Configuration["BackendUrl"] ?? "https://localhost:5001", 
            builder.Configuration["FrontendUrl"] ?? "https://localhost:5002"])
            .AllowAnyMethod()
            .SetIsOriginAllowed(pol => true)
            .AllowAnyHeader()
            .AllowCredentials()));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// create routes for the identity endpoints
app.MapIdentityApi<AppUser>();

// provide an end point to clear the cookie for logout
// The request checks for an empty body to prevent CSRF attacks. By requiring something
// in the body, the request must be made from JavaScript, which is the only way to
// access the cookie. It can't be accessed by a form-based post.
// This prevents a malicious site from logging the user out.
// Furthermore, the endpoint is protected by authorization to prevent anonymous access.
// The client simply needs to pass an empty object {} in the body of the request.
app.MapPost("/Logout", async (SignInManager<AppUser> signInManager, [FromBody] object empty) =>
{
    if (empty != null)
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }
    return Results.Unauthorized();
}).RequireAuthorization();

// activate the CORS policy
app.UseCors("wasm");

app.UseHttpsRedirection();
app.Run();

// identity user
class AppUser : IdentityUser { }

// identity database
class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
}
