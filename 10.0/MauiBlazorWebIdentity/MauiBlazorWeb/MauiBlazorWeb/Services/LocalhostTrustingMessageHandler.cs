namespace MauiBlazorWeb.Services;

public class LocalhostTrustingMessageHandler : DelegatingHandler
{
    public LocalhostTrustingMessageHandler() : base(Create())
    {
    }

    private static HttpMessageHandler Create()
    {
#if ANDROID
        return new Xamarin.Android.Net.AndroidMessageHandler
        {
            ServerCertificateCustomValidationCallback = static (message, cert, chain, errors) =>
            {
                if (cert != null && cert.Issuer.Equals("CN=localhost"))
                    return true;

                return errors == System.Net.Security.SslPolicyErrors.None;
            }
        };
#elif IOS || MACCATALYST
        return new NSUrlSessionHandler
        {
            TrustOverrideForUrl = static (sender, url, trust) =>
            {
                return url.StartsWith("https://localhost");
            }
        };
#else
        return new HttpClientHandler();
#endif
    }
}
