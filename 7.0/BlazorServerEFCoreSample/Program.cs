using Microsoft.EntityFrameworkCore;
using BlazorServerEFCoreSample.Data;
using BlazorServerEFCoreSample.Grid;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// register factory and configure the options
#region snippet1
builder.Services.AddDbContextFactory<ContactContext>(opt =>
    opt.UseSqlite($"Data Source={nameof(ContactContext.ContactsDb)}.db"));
#endregion

// pager
builder.Services.AddScoped<IPageHelper, PageHelper>();

// filters
builder.Services.AddScoped<IContactFilters, GridControls>();

// query adapter (applies filter to contact request).
builder.Services.AddScoped<GridQueryAdapter>();

// service to communicate success on edit between pages
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
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
