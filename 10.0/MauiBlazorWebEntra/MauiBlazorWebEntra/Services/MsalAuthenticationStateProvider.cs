using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Security.Claims;

namespace MauiBlazorWebEntra.Services;

/// <summary>
/// Authentication state provider that uses MSAL.NET to authenticate against
/// Microsoft Entra External ID. Handles interactive sign-in via system browser,
/// silent token acquisition, and sign-out.
/// </summary>
public class MsalAuthenticationStateProvider(IPublicClientApplication msalClient) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal _anonymousUser = new(new ClaimsIdentity());
    private ClaimsPrincipal _currentUser = _anonymousUser;
    private bool _initialized;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_initialized)
        {
            _initialized = true;
            await TrySignInSilentAsync();
        }

        return new AuthenticationState(_currentUser);
    }

    /// <summary>
    /// Attempts silent authentication using cached accounts, then falls back
    /// to interactive sign-in if no cached token is available.
    /// </summary>
    public async Task<bool> TrySignInSilentAsync()
    {
        try
        {
            var accounts = await msalClient.GetAccountsAsync();
            var account = accounts.FirstOrDefault();

            if (account is not null)
            {
                var result = await msalClient.AcquireTokenSilent(MsalConfig.Scopes, account)
                    .ExecuteAsync();

                SetUserFromResult(result);
                return true;
            }
        }
        catch (MsalUiRequiredException)
        {
            // Silent auth failed — user must sign in interactively
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Silent sign-in failed: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Starts an interactive sign-in flow via the system browser.
    /// </summary>
    public async Task SignInInteractiveAsync()
    {
        try
        {
            var result = await msalClient.AcquireTokenInteractive(MsalConfig.Scopes)
#if ANDROID
                .WithParentActivityOrWindow(Platform.CurrentActivity)
#elif IOS || MACCATALYST
                .WithSystemWebViewOptions(new SystemWebViewOptions())
#elif WINDOWS
                .WithParentActivityOrWindow(GetCurrentWindowHandle())
#endif
                .ExecuteAsync();

            SetUserFromResult(result);
        }
        catch (MsalClientException ex) when (ex.ErrorCode == "authentication_canceled")
        {
            Console.WriteLine($"MSAL canceled: {ex.ErrorCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MSAL error: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"MSAL inner: {ex.InnerException.Message}");
        }
    }

    /// <summary>
    /// Signs out the current user and clears the MSAL token cache.
    /// </summary>
    public async Task SignOutAsync()
    {
        var accounts = await msalClient.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await msalClient.RemoveAsync(account);
        }

        _currentUser = _anonymousUser;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    /// Gets a valid access token for API calls. Returns null if not authenticated.
    /// </summary>
    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var accounts = await msalClient.GetAccountsAsync();
            var account = accounts.FirstOrDefault();

            if (account is null)
                return null;

            var result = await msalClient.AcquireTokenSilent(MsalConfig.Scopes, account)
                .ExecuteAsync();

            return result.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            // Token expired and refresh failed — user needs to sign in again
            await SignOutAsync();
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to get access token: {ex.Message}");
            return null;
        }
    }

    private void SetUserFromResult(AuthenticationResult result)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.Name, result.Account.Username ?? "User"),
        ];

        // Add ID token claims if available
        if (result.ClaimsPrincipal?.Claims is not null)
        {
            foreach (var claim in result.ClaimsPrincipal.Claims)
            {
                if (!claims.Any(c => c.Type == claim.Type))
                {
                    claims.Add(claim);
                }
            }
        }

        var identity = new ClaimsIdentity(claims, "Entra External ID");
        _currentUser = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

#if WINDOWS
    private static nint GetCurrentWindowHandle()
    {
        var window = (Microsoft.UI.Xaml.Window)App.Current.Windows[0].Handler!.PlatformView!;
        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        return windowHandle;
    }
#endif
}
