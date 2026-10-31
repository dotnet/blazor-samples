using Android.App;
using Android.Content;
using MauiBlazorWebEntra.Services;
using Microsoft.Identity.Client;

namespace MauiBlazorWebEntra;

[Activity(Exported = true)]
[IntentFilter([Intent.ActionView],
    Categories = [Intent.CategoryBrowsable, Intent.CategoryDefault],
    DataHost = "auth",
    DataSchemes = ["msal" + MsalClient.ExternalIdClientId, "msal" + MsalClient.WorkforceIdClientId])]
public class MsalActivity : BrowserTabActivity
{
}
