using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace MauiBlazorWebEntra.Platforms.Android
{
    /// <summary>
    /// Activity that handles the MSAL redirect URI callback from the system browser
    /// after Entra External ID authentication completes.
    /// </summary>
    [Activity(Exported = true)]
    [IntentFilter(
        [Intent.ActionView],
        Categories = [Intent.CategoryBrowsable, Intent.CategoryDefault],
        DataScheme = "msalYOUR_MAUI_CLIENT_ID",
        DataHost = "auth")]
    public class MsalActivity : BrowserTabActivity
    {
    }
}
