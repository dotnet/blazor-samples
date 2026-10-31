using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace MauiBlazorWebEntra.Services;

public sealed class MsalAuthenticationService(
    IPublicClientApplication msalClient, IConfiguration configuration, ILogger<MsalAuthenticationService> logger) : IDisposable
{
    private static readonly ClaimsPrincipal anonymousUser = new(new ClaimsIdentity());
    private readonly SemaphoreSlim sessionLock = new(1, 1);
    private bool initialized;

    public ClaimsPrincipal User { get; private set; } = anonymousUser;
    public event Action<ClaimsPrincipal>? UserChanged;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await sessionLock.WaitAsync(cancellationToken);
        var previousUser = User;
        try
        {
            if (!initialized)
            {
                await AcquireTokenAsync(interactive: false, cancellationToken);
            }
        }
        finally
        {
            sessionLock.Release();
            NotifyUserChanged(previousUser);
        }
    }

    public async Task SignInAsync(CancellationToken cancellationToken = default)
    {
        await sessionLock.WaitAsync(cancellationToken);
        var previousUser = User;
        try
        {
            await AcquireTokenAsync(interactive: true, cancellationToken);
        }
        finally
        {
            sessionLock.Release();
            NotifyUserChanged(previousUser);
        }
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        await sessionLock.WaitAsync(cancellationToken);
        var previousUser = User;
        try
        {
            var result = await AcquireTokenAsync(interactive: false, cancellationToken);
            return result?.AccessToken;
        }
        finally
        {
            sessionLock.Release();
            NotifyUserChanged(previousUser);
        }
    }

    private async Task<AuthenticationResult?> AcquireTokenAsync(bool interactive, CancellationToken cancellationToken)
    {
        var scopes = configuration.GetRequiredSection("Entra:Scopes").GetChildren()
            .Select(scope => scope.Value
                ?? throw new InvalidOperationException($"Configuration '{scope.Path}' is missing."))
            .ToArray();
        if (scopes.Length == 0 || scopes.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("Configuration 'Entra:Scopes' must contain at least one non-empty API scope.");
        }

        var account = (await msalClient.GetAccountsAsync()).FirstOrDefault();
        AuthenticationResult? result = null;
        string? claims = null;

        if (account is not null)
        {
            try
            {
                result = await msalClient.AcquireTokenSilent(scopes, account)
                    .ExecuteAsync(cancellationToken);
            }
            catch (MsalUiRequiredException ex)
            {
                logger.LogInformation("Sign-in requires user interaction: {Classification}.", ex.Classification);
                claims = ex.Claims;
            }
        }

        if (result is null && interactive)
        {
            result = await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var request = msalClient.AcquireTokenInteractive(scopes)
                    .WithAccount(account)
                    .WithPlatformBrowser();

                if (!string.IsNullOrEmpty(claims))
                {
                    request = request.WithClaims(claims);
                }

                return request.ExecuteAsync(cancellationToken);
            });
        }

        initialized = true;
        if (result is null)
        {
            User = anonymousUser;
        }
        else
        {
            var identity = new ClaimsIdentity(result.ClaimsPrincipal?.Claims ?? [], "MSAL", "name", "roles");
            if (string.IsNullOrEmpty(identity.Name))
            {
                identity.AddClaim(new Claim("name", result.Account.Username ?? "Signed-in user"));
            }
            User = new ClaimsPrincipal(identity);
        }

        return result;
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        await sessionLock.WaitAsync(cancellationToken);
        var previousUser = User;
        try
        {
            foreach (var account in await msalClient.GetAccountsAsync())
            {
                await msalClient.RemoveAsync(account);
            }
            User = anonymousUser;
            initialized = true;
        }
        finally
        {
            sessionLock.Release();
            NotifyUserChanged(previousUser);
        }
    }

    private void NotifyUserChanged(ClaimsPrincipal previousUser)
    {
        if (!ReferenceEquals(previousUser, User))
        {
            UserChanged?.Invoke(User);
        }
    }

    public void Dispose() => sessionLock.Dispose();
}
