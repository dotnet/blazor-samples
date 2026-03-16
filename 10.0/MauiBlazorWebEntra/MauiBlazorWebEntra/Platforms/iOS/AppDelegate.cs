using Foundation;
using Microsoft.Identity.Client;
using UIKit;

namespace MauiBlazorWebEntra
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
        {
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
            return base.OpenUrl(application, url, options);
        }
    }
}
