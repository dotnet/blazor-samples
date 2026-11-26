using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace MauiBlazorWebEntra.Services;

public sealed class MsalAuthenticationService(MsalClient msalClient, ILogger<MsalAuthenticationService> logger) : IDisposable
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
        var account = (await msalClient.Application.GetAccountsAsync()).FirstOrDefault();
        AuthenticationResult? result = null;
        string? claims = null;

        if (account is not null)
        {
            try
            {
                result = await msalClient.Application.AcquireTokenSilent(msalClient.Configuration.Scopes, account)
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
                var request = msalClient.AcquireTokenInteractive()
                    .WithAccount(account);

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
            foreach (var account in await msalClient.Application.GetAccountsAsync())
            {
                await msalClient.Application.RemoveAsync(account);
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
