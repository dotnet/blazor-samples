namespace MauiBlazorWeb.Shared.Services;

public interface ISupportedFeatures
{
    public const string AccountManagement = nameof(AccountManagement);

    bool IsFeatureSupported(string featureName);
}
