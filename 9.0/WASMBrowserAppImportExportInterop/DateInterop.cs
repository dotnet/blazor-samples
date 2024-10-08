using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public partial class DateInterop
{
    [JSImport("incrementDay", "DateShim")]
    [return: JSMarshalAs<JSType.Date>] // Explicit JSMarshalAs for a return type
    public static partial DateTime IncrementDay(
        [JSMarshalAs<JSType.Date>] DateTime date);

    [JSImport("logValueAndType", "DateShim")]
    public static partial void LogValueAndType(
        [JSMarshalAs<JSType.Date>] DateTime value);
}

public static class DateUsage
{
    public static async Task Run()
    {
        // Ensure JS module loaded.
        await JSHost.ImportAsync("DateShim", "/DateShim.js");

        // Basic examples of interop with a C# DateTime and JS Date.
        DateTime date = new(1968, 12, 21, 12, 51, 0, DateTimeKind.Utc);
        DateInterop.LogValueAndType(date);
        date = DateInterop.IncrementDay(date);
        DateInterop.LogValueAndType(date);
    }
}
