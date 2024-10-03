using MauiBlazorWeb.Shared.Interfaces;

namespace MauiBlazorWeb.Web.Services;

public class FormFactor : IFormFactor
{
    public string GetFormFactor() => "Web";

    public string GetPlatform() => Environment.OSVersion.ToString();
}
