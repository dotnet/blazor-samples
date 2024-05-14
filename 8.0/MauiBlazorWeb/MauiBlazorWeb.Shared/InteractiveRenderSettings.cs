using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MauiBlazorWeb.Shared;

public static class InteractiveRenderSettings
{
    public static IComponentRenderMode? InteractiveServer { get; set; } = 
        RenderMode.InteractiveServer;
    public static IComponentRenderMode? InteractiveAuto { get; set; } = 
        RenderMode.InteractiveAuto;
    public static IComponentRenderMode? InteractiveWebAssembly { get; set; } = 
        RenderMode.InteractiveWebAssembly;

    public static void ConfigureBlazorHybridRenderModes()
    {
        InteractiveServer = null;
        InteractiveAuto = null;
        InteractiveWebAssembly = null;
    }
}
