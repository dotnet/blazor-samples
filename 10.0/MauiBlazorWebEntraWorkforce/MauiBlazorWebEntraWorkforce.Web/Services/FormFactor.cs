using MauiBlazorWebEntraWorkforce.Shared.Services;

namespace MauiBlazorWebEntraWorkforce.Web.Services;

public class FormFactor : IFormFactor
{
    public string GetFormFactor()
    {
        return "Web";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }
}
