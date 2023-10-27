using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// cookie authentication
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();

// configure authorization
builder.Services.AddAuthorizationBuilder();

// add the database (in memory for the sample)
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("AppDb"));

// add identity and opt-in to endpoints
builder.Services.AddIdentityCore<MyUser>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

// add CORS policy for Wasm client
builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.WithOrigins([builder.Configuration["BackendUrl"], builder.Configuration["FrontendUrl"]])
            .AllowAnyMethod()
            .SetIsOriginAllowed(pol => true)
            .AllowAnyHeader()
            .AllowCredentials()));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// create routes for the identity endpoints
app.MapIdentityApi<MyUser>();

// provide an end point to clear the cookie for logout
app.MapPost("/Logout", async (
           ClaimsPrincipal user,
           SignInManager<MyUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return TypedResults.Ok();
});

// activate the CORS policy
app.UseCors("wasm");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();

// identity user
class MyUser : IdentityUser { }

// identity database
class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<MyUser>(options)
{
}
