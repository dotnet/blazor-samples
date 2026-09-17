#if DEBUG
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace MauiBlazorWeb;

internal static class DevFlowWebViewInspector
{
    private static int enabled;

    internal static void Enable()
    {
        if (Interlocked.Exchange(ref enabled, 1) != 0)
        {
            return;
        }

        BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping(
            "DevFlowInspectable",
            static (handler, _) =>
            {
#if IOS
                if (handler.PlatformView is WebKit.WKWebView webView &&
                    OperatingSystem.IsIOSVersionAtLeast(16, 4))
                {
                    webView.Inspectable = true;
                }
#elif MACCATALYST
                if (handler.PlatformView is WebKit.WKWebView webView &&
                    OperatingSystem.IsMacCatalystVersionAtLeast(16, 4))
                {
                    webView.Inspectable = true;
                }
#endif
            });
    }
}
#endif
