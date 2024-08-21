using System.Security.Claims;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;

namespace BlazorWebAssemblyEntraGroupsAndRoles;

public class CustomAccountFactory(IAccessTokenProviderAccessor accessor,
        IServiceProvider serviceProvider, ILogger<CustomAccountFactory> logger,
        IConfiguration config)
    : AccountClaimsPrincipalFactory<CustomUserAccount>(accessor)
{
    private readonly ILogger<CustomAccountFactory> logger = logger;
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly string? baseUrl = string.Join("/",
        config.GetSection("MicrosoftGraph")["BaseUrl"],
        config.GetSection("MicrosoftGraph")["Version"]);

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
        CustomUserAccount account,
        RemoteAuthenticationUserOptions options)
    {
        var initialUser = await base.CreateUserAsync(account, options);

        if (initialUser.Identity is not null &&
            initialUser.Identity.IsAuthenticated)
        {
            var userIdentity = initialUser.Identity as ClaimsIdentity;

            if (userIdentity is not null && !string.IsNullOrEmpty(baseUrl) &&
                account.Oid is not null)
            {
                account?.Roles?.ForEach((role) =>
                {
                    userIdentity.AddClaim(new Claim("role", role));
                });

                try
                {
                    var client = new GraphServiceClient(
                        new HttpClient(),
                        serviceProvider
                            .GetRequiredService<IAuthenticationProvider>(),
                        baseUrl);

                    var user = await client.Me.GetAsync();

                    if (user is not null)
                    {
                        userIdentity.AddClaim(new Claim("mobilephone",
                            user.MobilePhone ?? "(000) 000-0000"));
                        userIdentity.AddClaim(new Claim("officelocation",
                            user.OfficeLocation ?? "Not set"));
                    }

                    var memberOf = client.Users[account?.Oid].MemberOf;

                    var graphDirectoryRoles = await memberOf.GraphDirectoryRole.GetAsync();

                    if (graphDirectoryRoles?.Value is not null)
                    {
                        foreach (var entry in graphDirectoryRoles.Value)
                        {
                            if (entry.RoleTemplateId is not null)
                            {
                                userIdentity.AddClaim(
                                    new Claim("directoryRole", entry.RoleTemplateId));
                            }
                        }
                    }

                    var graphAdministrativeUnits = await memberOf.GraphAdministrativeUnit.GetAsync();

                    if (graphAdministrativeUnits?.Value is not null)
                    {
                        foreach (var entry in graphAdministrativeUnits.Value)
                        {
                            if (entry.Id is not null)
                            {
                                userIdentity.AddClaim(
                                    new Claim("administrativeUnit", entry.Id));
                            }
                        }
                    }

                    var graphGroups = await memberOf.GraphGroup.GetAsync();

                    if (graphGroups?.Value is not null)
                    {
                        foreach (var entry in graphGroups.Value)
                        {
                            if (entry.Id is not null)
                            {
                                userIdentity.AddClaim(
                                    new Claim("directoryGroup", entry.Id));
                            }
                        }
                    }
                }
                catch (AccessTokenNotAvailableException exception)
                {
                    exception.Redirect();
                }
            }
        }

        return initialUser;
    }
}
