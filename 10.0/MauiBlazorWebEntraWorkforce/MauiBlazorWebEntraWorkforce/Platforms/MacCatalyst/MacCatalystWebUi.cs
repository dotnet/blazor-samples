using AuthenticationServices;
using Foundation;
using Microsoft.Identity.Client.Extensibility;
using UIKit;

namespace Microsoft.Identity.Client;

/// <summary>
/// Extension method to configure Mac Catalyst authentication using
/// ASWebAuthenticationSession. MSAL doesn't ship a maccatalyst TFM yet,
/// so the built-in system browser flow throws PlatformNotSupportedException.
/// This provides an equivalent experience using ICustomWebUi.
///
/// Remove this file once MSAL ships Mac Catalyst support:
/// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/3527
/// </summary>
internal static class MacCatalystWebViewExtensions
{
    /// <summary>
    /// Configures the interactive token request to use ASWebAuthenticationSession
    /// on Mac Catalyst, working around the missing maccatalyst TFM in MSAL.
    /// Replace with <c>.WithSystemWebViewOptions(new SystemWebViewOptions())</c>
    /// once MSAL ships Mac Catalyst support.
    /// </summary>
    public static AcquireTokenInteractiveParameterBuilder WithMacCatalystWebView(
        this AcquireTokenInteractiveParameterBuilder builder) =>
        builder.WithCustomWebUi(new MacCatalystWebUi());

    private class MacCatalystWebUi : ICustomWebUi
    {
        public async Task<Uri> AcquireAuthorizationCodeAsync(
            Uri authorizationUri, Uri redirectUri, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<Uri>();

            using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());

            var callbackScheme = redirectUri.Scheme;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                // The callback-based constructor is deprecated on macCat 17.4+ in favor
                // of ASWebAuthenticationSessionCallback, but remains the simplest approach
                // for broad compatibility. Suppress the warning until MSAL ships native
                // Mac Catalyst support and this file can be deleted entirely.
#pragma warning disable CA1422
                var session = new ASWebAuthenticationSession(
                    new NSUrl(authorizationUri.AbsoluteUri),
                    callbackScheme,
                    (callbackUrl, error) =>
                    {
                        if (error is not null)
                        {
                            if (error.Code == (long)ASWebAuthenticationSessionErrorCode.CanceledLogin)
                                tcs.TrySetException(new MsalClientException(
                                    "authentication_canceled", "User canceled authentication."));
                            else
                                tcs.TrySetException(new Exception(
                                    $"ASWebAuthenticationSession error: {error.LocalizedDescription}"));
                        }
                        else if (callbackUrl is not null)
                        {
                            tcs.TrySetResult(new Uri(callbackUrl.ToString()));
                        }
                        else
                        {
                            tcs.TrySetException(new Exception(
                                "No callback URL received from ASWebAuthenticationSession."));
                        }
                    });
#pragma warning restore CA1422

                session.PresentationContextProvider = new PresentationContextProvider();
                session.PrefersEphemeralWebBrowserSession = false;

                if (!session.Start())
                {
                    tcs.TrySetException(new Exception("Failed to start ASWebAuthenticationSession."));
                }
            });

            return await tcs.Task;
        }

        private class PresentationContextProvider : NSObject, IASWebAuthenticationPresentationContextProviding
        {
            public UIWindow GetPresentationAnchor(ASWebAuthenticationSession session)
            {
                var scene = UIApplication.SharedApplication.ConnectedScenes
                    .OfType<UIWindowScene>()
                    .FirstOrDefault();

                return scene?.KeyWindow
                    ?? scene?.Windows.FirstOrDefault()
                    ?? throw new InvalidOperationException("No window found for authentication presentation.");
            }
        }
    }
}
