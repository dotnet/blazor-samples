using MauiBlazorWeb.Shared.Services;

namespace MauiBlazorWeb.Services;

public class SupportedFeatures : ISupportedFeatures
{
    public bool IsFeatureSupported(string featureName) =>
        featureName switch
        {
            ISupportedFeatures.AccountManagement => true,
            _ => false,
        };
}
