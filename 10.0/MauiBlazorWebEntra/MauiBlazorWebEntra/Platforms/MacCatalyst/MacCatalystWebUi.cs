using AuthenticationServices;
using Foundation;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensibility;
using UIKit;

namespace MauiBlazorWebEntra.Platforms.MacCatalyst
{
    /// <summary>
    /// Custom web UI for Mac Catalyst that uses ASWebAuthenticationSession.
    /// MSAL's built-in OS browser flow doesn't support Mac Catalyst, so we
    /// implement ICustomWebUi to handle the browser-based auth flow natively.
    /// </summary>
    public class MacCatalystWebUi : ICustomWebUi
    {
        public async Task<Uri> AcquireAuthorizationCodeAsync(
            Uri authorizationUri, Uri redirectUri, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<Uri>();

            using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());

            // For http://localhost redirects, use the "http" scheme so
            // ASWebAuthenticationSession intercepts the localhost callback.
            var callbackScheme = redirectUri.Scheme;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var contextProvider = new PresentationContextProvider();

#pragma warning disable CA1422 // Validate platform compatibility
                var session = new ASWebAuthenticationSession(
                    new NSUrl(authorizationUri.AbsoluteUri),
                    callbackScheme,
                    (callbackUrl, error) =>
                    {
                        if (error != null)
                        {
                            if (error.Code == (long)ASWebAuthenticationSessionErrorCode.CanceledLogin)
                                tcs.TrySetException(new MsalClientException("authentication_canceled", "User canceled authentication."));
                            else
                                tcs.TrySetException(new Exception($"ASWebAuthenticationSession error: {error.LocalizedDescription}"));
                        }
                        else if (callbackUrl != null)
                        {
                            tcs.TrySetResult(new Uri(callbackUrl.ToString()));
                        }
                        else
                        {
                            tcs.TrySetException(new Exception("No callback URL received from ASWebAuthenticationSession."));
                        }
                    });
#pragma warning restore CA1422

                session.PresentationContextProvider = contextProvider;
                session.PrefersEphemeralWebBrowserSession = false;

                if (!session.Start())
                {
                    tcs.TrySetException(new Exception("Failed to start ASWebAuthenticationSession."));
                }
            });

            return await tcs.Task;
        }
    }

    /// <summary>
    /// Provides the presentation anchor (window) for ASWebAuthenticationSession.
    /// Must inherit from NSObject to satisfy the ObjC protocol binding.
    /// </summary>
    public class PresentationContextProvider : NSObject, IASWebAuthenticationPresentationContextProviding
    {
        public UIWindow GetPresentationAnchor(ASWebAuthenticationSession session)
        {
            var scene = UIApplication.SharedApplication.ConnectedScenes
                .OfType<UIWindowScene>()
                .FirstOrDefault();

            return scene?.KeyWindow
                ?? scene?.Windows.FirstOrDefault()
                ?? UIApplication.SharedApplication.Windows.First();
        }
    }
}
