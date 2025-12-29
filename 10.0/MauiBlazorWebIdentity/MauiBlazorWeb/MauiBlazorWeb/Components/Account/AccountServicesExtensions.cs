using Microsoft.AspNetCore.Components.Authorization;

namespace MauiBlazorWeb.Components.Account;

public static class AccountServicesExtensions
{
    public static IServiceCollection AddAccountServices(this IServiceCollection services, string routeGroupEndpoint, Action<IHttpClientBuilder>? configureClient = null)
    {
        // Register the custom AuthenticationStateProvider
        services.AddSingleton<MauiAuthenticationStateProvider>();

        // Use the custom provider when the app needs an AuthenticationStateProvider
        services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<MauiAuthenticationStateProvider>());

        // Use the custom provider when the app needs an ISignInManager
        services.AddSingleton<ISignInManager>(sp => sp.GetRequiredService<MauiAuthenticationStateProvider>());

        // Register the redirect manager
        services.AddScoped<IdentityRedirectManager>();

        // Register the authentication handler for HttpClient
        services.AddTransient<IdentityAuthorizationMessageHandler>();

        // Configure Typed HttpClient for IdentityApiClient
        var clientBuilder = services.AddHttpClient<IdentityApiClient>(client =>
            {
                client.BaseAddress = new Uri(routeGroupEndpoint);
            });
        configureClient?.Invoke(clientBuilder);

        return services;
    }

    public static IHttpClientBuilder AddIdentityAuthorizationHandler(this IHttpClientBuilder builder)
    {
        return builder.AddHttpMessageHandler<IdentityAuthorizationMessageHandler>();
    }
}
