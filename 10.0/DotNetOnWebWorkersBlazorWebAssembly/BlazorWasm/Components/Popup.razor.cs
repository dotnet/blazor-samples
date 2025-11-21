using Microsoft.AspNetCore.Components;
using System.Runtime.Versioning;

namespace Components;

[SupportedOSPlatform("browser")]
public partial class Popup : ComponentBase
{
    private string message = string.Empty;
    private string title = string.Empty;
    private bool isVisible;

    public void Show(string title, string message)
    {
        Console.WriteLine($"Popup.Show: {title} - {message}");
        this.title = title;
        this.message = message;
        isVisible = true;
        InvokeAsync(StateHasChanged);
    }

    public void Hide()
    {
        isVisible = false;
        InvokeAsync(StateHasChanged);
    }
}
