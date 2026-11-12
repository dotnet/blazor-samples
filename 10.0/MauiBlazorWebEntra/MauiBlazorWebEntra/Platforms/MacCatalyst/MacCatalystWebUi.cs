using AuthenticationServices;
using Foundation;
using Microsoft.Identity.Client.Extensibility;
using UIKit;

namespace Microsoft.Identity.Client;

// MSAL doesn't ship a Mac Catalyst target. Use the tutorial's system browser adapter.
// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/3527
internal static class MacCatalystWebViewExtensions
{
    public static AcquireTokenInteractiveParameterBuilder WithMacCatalystWebView(
        this AcquireTokenInteractiveParameterBuilder builder) =>
        builder.WithCustomWebUi(new MacCatalystWebUi());

    private sealed class MacCatalystWebUi : ICustomWebUi
    {
        public Task<Uri> AcquireAuthorizationCodeAsync(
            Uri authorizationUri, Uri redirectUri, CancellationToken cancellationToken) =>
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var completion = new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously);
                using var presentationContext = new PresentationContextProvider();

                // This constructor also supports the sample's minimum Mac Catalyst 17.0.
#pragma warning disable CA1422
                using var session = new ASWebAuthenticationSession(
                    new NSUrl(authorizationUri.AbsoluteUri),
                    redirectUri.Scheme,
                    (callbackUrl, error) =>
                    {
                        if (error is not null)
                        {
                            completion.TrySetException(new MsalClientException(
                                error.Code == (long)ASWebAuthenticationSessionErrorCode.CanceledLogin
                                    ? "authentication_canceled" : "authentication_failed",
                                error.LocalizedDescription));
                        }
                        else if (Uri.TryCreate(callbackUrl?.AbsoluteString, UriKind.Absolute, out var callback))
                        {
                            completion.TrySetResult(callback);
                        }
                        else
                        {
                            completion.TrySetException(new MsalClientException(
                                "authentication_failed", "The system browser returned no valid callback URL."));
                        }
                    });
#pragma warning restore CA1422

                session.PresentationContextProvider = presentationContext;
                session.PrefersEphemeralWebBrowserSession = false;

                using var registration = cancellationToken.Register(() =>
                    completion.TrySetCanceled(cancellationToken));

                if (!session.Start())
                {
                    throw new MsalClientException("authentication_failed", "Unable to start the system browser.");
                }

                try
                {
                    return await completion.Task;
                }
                finally
                {
                    session.Cancel();
                }
            });

        private sealed class PresentationContextProvider : NSObject, IASWebAuthenticationPresentationContextProviding
        {
            public UIWindow GetPresentationAnchor(ASWebAuthenticationSession session)
            {
                var scene = UIApplication.SharedApplication.ConnectedScenes
                    .OfType<UIWindowScene>()
                    .FirstOrDefault(scene => scene.ActivationState == UISceneActivationState.ForegroundActive);

                return scene?.KeyWindow ?? scene?.Windows.FirstOrDefault()
                    ?? throw new InvalidOperationException("No window is available for sign-in.");
            }
        }
    }
}
