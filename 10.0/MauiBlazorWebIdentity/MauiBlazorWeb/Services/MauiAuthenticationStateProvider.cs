using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace MauiBlazorWeb.Services
{
    public sealed class MauiAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
    {
        private readonly MauiSignInManager signInManager;

        public MauiAuthenticationStateProvider(MauiSignInManager manager)
        {
            signInManager = manager;
            signInManager.UserChanged += OnUserChanged;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            await signInManager.InitializeAsync();
            return new AuthenticationState(signInManager.User);
        }

        private void OnUserChanged(ClaimsPrincipal user) =>
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

        public void Dispose() =>
            signInManager.UserChanged -= OnUserChanged;
    }
}
