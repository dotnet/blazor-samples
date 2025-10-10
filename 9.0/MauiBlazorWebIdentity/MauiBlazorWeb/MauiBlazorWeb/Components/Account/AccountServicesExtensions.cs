using Microsoft.AspNetCore.Components.Authorization;

namespace MauiBlazorWeb.Components.Account;

public static class AccountServicesExtensions
{
    public static IServiceCollection AddAccountServices(this IServiceCollection services, string routeGroupEndpoint, Action<IHttpClientBuilder>? configureClient = null)
    {
        // Register a custom AuthenticationStateProvider
        services.AddSingleton<MauiAuthenticationStateProvider>();

        // Use the custom provider when the app needs an AuthenticationStateProvider
        services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<MauiAuthenticationStateProvider>());

        // Register the authentication handler for HttpClient
        services.AddTransient<AuthorizationMessageHandler>();

        // Register the sign in service used by the AuthenticationStateProvider
        services.AddScoped<SignInService>();

        // Configure Typed HttpClient for SignInService
        var clientBuilder = services.AddHttpClient<SignInService>(client =>
            {
                client.BaseAddress = new Uri(routeGroupEndpoint);
            });
        configureClient?.Invoke(clientBuilder);

        return services;
    }

    public static IHttpClientBuilder AddAuthorizationHandler(this IHttpClientBuilder builder)
    {
        return builder.AddHttpMessageHandler<AuthorizationMessageHandler>();
    }
}
