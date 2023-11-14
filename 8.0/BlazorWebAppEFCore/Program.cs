using BlazorWebAppEFCore.Components;
using Microsoft.EntityFrameworkCore;
using BlazorWebAppEFCore.Data;
using BlazorWebAppEFCore.Grid;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register factory and configure the options
#region snippet1
builder.Services.AddDbContextFactory<ContactContext>(opt =>
    opt.UseSqlite($"Data Source={nameof(ContactContext.ContactsDb)}.db"));
#endregion

// Pager
builder.Services.AddScoped<IPageHelper, PageHelper>();

// Filters
builder.Services.AddScoped<IContactFilters, GridControls>();

// Query adapter (applies filter to contact request)
builder.Services.AddScoped<GridQueryAdapter>();

// Service to communicate success on edit between pages
builder.Services.AddScoped<EditSuccess>();

var app = builder.Build();

// This section sets up and seeds the database. Seeding is NOT normally
// handled this way in production. The following approach is used in this
// sample app to make the sample simpler. The app can be cloned. The
// connection string is configured. The app can be run.
await using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<ContactContext>>();
await DatabaseUtility.EnsureDbCreatedAndSeedWithCountOfAsync(options, 500);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
