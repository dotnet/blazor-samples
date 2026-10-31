using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace MauiBlazorWebEntra;

[Activity(Exported = true)]
[IntentFilter([Intent.ActionView],
    Categories = [Intent.CategoryBrowsable, Intent.CategoryDefault],
    DataHost = "auth",
    DataSchemes = ["msal" + MsalConfig.ExternalIdClientId, "msal" + MsalConfig.WorkforceIdClientId])]
public class MsalActivity : BrowserTabActivity
{
}
