using MauiBlazorWeb.Services;
using MauiBlazorWeb.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace MauiBlazorWeb
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.ConfigureBackend();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton(SecureStorage.Default);
            builder.Services.AddSingleton<IdentityApiClient>();
            builder.Services.AddSingleton<MauiSignInManager>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<MauiAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<MauiAuthenticationStateProvider>());

            // Login and refresh use the default client without the bearer-token handler.
            builder.Services.AddTransient<IdentityBearerTokenHandler>();
            builder.Services.AddHttpClient<IWeatherService, WeatherService>()
                .AddHttpMessageHandler<IdentityBearerTokenHandler>();

            // Add device-specific services used by the MauiBlazorWeb.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            return builder.Build();
        }

        private static MauiAppBuilder ConfigureBackend(this MauiAppBuilder builder)
        {
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
#if ANDROID && DEBUG
                ["BackendUrl"] = "https://10.0.2.2:7157/"
#else
                ["BackendUrl"] = "https://localhost:7157/"
#endif
            });

            builder.Services.ConfigureHttpClientDefaults(http =>
                http.ConfigureHttpClient((services, client) =>
                {
                    var configuration = services.GetRequiredService<IConfiguration>();
                    client.BaseAddress = new Uri(configuration["BackendUrl"]!, UriKind.Absolute);
                    client.Timeout = TimeSpan.FromSeconds(30);
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
#if ANDROID
                    var handler = new Xamarin.Android.Net.AndroidMessageHandler();
#if DEBUG
                    handler.ServerCertificateCustomValidationCallback = (request, certificate, chain, errors) =>
                        errors == System.Net.Security.SslPolicyErrors.None
                        || (request.RequestUri is { } uri && (uri.IsLoopback || uri.Host == "10.0.2.2")
                            && certificate?.Issuer == "CN=localhost");
#endif
                    return handler;
#elif IOS
                    var handler = new NSUrlSessionHandler();
#if DEBUG
                    handler.TrustOverrideForUrl = (sender, url, trust) =>
                        Uri.TryCreate(url, UriKind.Absolute, out var uri)
                        && uri.Scheme == Uri.UriSchemeHttps && uri.IsLoopback;
#endif
                    return handler;
#else
                    return new HttpClientHandler();
#endif
                }));

            return builder;
        }
    }
}
