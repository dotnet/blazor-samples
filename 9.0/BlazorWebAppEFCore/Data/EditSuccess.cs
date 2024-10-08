namespace BlazorWebAppEFCore.Data;

// Service to communicate success status between pages.
public class EditSuccess
{
    // True when the last edit operation was successful.
    public bool Success { get; set; }
}
