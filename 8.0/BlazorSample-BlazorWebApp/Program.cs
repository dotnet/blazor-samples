using BlazorSample;
using BlazorSample.Components;
using static BlazorSample.Components.Pages.ProductDetails;
using static BlazorSample.Components.Pages.TheSunmakers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<NotifierService>();
builder.Services.AddScoped<TimerService>();
builder.Services.AddScoped<IDataAccess, DataAccess>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddHttpClient();

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
