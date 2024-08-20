using System.Security.Claims;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;

namespace StandMEID80GraphSDK;

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

            if (userIdentity is not null && !string.IsNullOrEmpty(baseUrl))
            {
                account?.Roles?.ForEach((role) =>
                {
                    userIdentity.AddClaim(new Claim("appRole", role));
                });

                account?.Wids?.ForEach((wid) =>
                {
                    userIdentity.AddClaim(new Claim("directoryRole", wid));
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

                    var requestMemberOf = client.Users[account?.Oid].MemberOf;

                    /* HOLDING ... for future coverage ...

                    var graphAdministrativeUnits = await requestMemberOf.GraphAdministrativeUnit.GetAsync();

                    if (graphAdministrativeUnits?.Value is not null)
                    {
                        foreach (var entry in graphAdministrativeUnits.Value)
                        {
                            if (entry.Id is not null)
                            {
                                userIdentity.AddClaim(
                                    new Claim("graphAdministrativeUnit", entry.Id));
                            }
                        }
                    }

                    var graphDirectoryRoles = await requestMemberOf.GraphDirectoryRole.GetAsync();

                    if (graphDirectoryRoles?.Value is not null)
                    {
                        foreach (var entry in graphDirectoryRoles.Value)
                        {
                            if (entry.Id is not null)
                            {
                                userIdentity.AddClaim(
                                    new Claim("graphDirectoryRole", entry.Id));
                            }
                        }
                    }
                    */

                    var graphGroups = await requestMemberOf.GraphGroup.GetAsync();

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
