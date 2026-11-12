using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace MauiBlazorWebEntra.Services
{
    public sealed class MauiAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
    {
        private readonly MsalAuthenticationService authentication;

        public MauiAuthenticationStateProvider(MsalAuthenticationService authenticationService)
        {
            authentication = authenticationService;
            authentication.UserChanged += OnUserChanged;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            await authentication.InitializeAsync();
            return new AuthenticationState(authentication.User);
        }

        private void OnUserChanged(ClaimsPrincipal user) =>
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

        public void Dispose() =>
            authentication.UserChanged -= OnUserChanged;
    }
}
