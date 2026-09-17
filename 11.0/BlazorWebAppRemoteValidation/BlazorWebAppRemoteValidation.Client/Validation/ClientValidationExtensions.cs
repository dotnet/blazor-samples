using Microsoft.Extensions.DependencyInjection;

namespace BlazorWebAppRemoteValidation.Client.Validation;

public static class ClientValidationExtensions
{
    public static IServiceCollection AddClientModelValidation(
        this IServiceCollection services) =>
        services.AddValidation();
}
