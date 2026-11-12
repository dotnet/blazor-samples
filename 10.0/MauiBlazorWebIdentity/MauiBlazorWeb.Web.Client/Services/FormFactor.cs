using MauiBlazorWeb.Shared.Services;

namespace MauiBlazorWeb.Web.Client.Services;

public sealed class FormFactor : IFormFactor
{
    public string GetFormFactor() => "Web";

    public string GetPlatform() => "WebAssembly";
}
