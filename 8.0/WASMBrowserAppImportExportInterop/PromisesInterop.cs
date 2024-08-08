using System;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public partial class PromisesInterop
{
    // For a promise with void return type, declare a Task return type:
    [JSImport("wait2Seconds", "PromisesShim")]
    public static partial Task Wait2Seconds();

    [JSImport("waitGetString", "PromisesShim")]
    public static partial Task<string> WaitGetString();

    // Some return types require a [return: JSMarshalAs...] declaring the
    // Promise's return type corresponding to Task<T>.
    [JSImport("waitGetDate", "PromisesShim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Date>>()]
    public static partial Task<DateTime> WaitGetDate();

    [JSImport("fetchCurrentUrl", "PromisesShim")]
    public static partial Task<string> FetchCurrentUrl();

    [JSImport("asyncFunction", "PromisesShim")]
    public static partial Task AsyncFunction();

    [JSImport("conditionalSuccess", "PromisesShim")]
    public static partial Task ConditionalSuccess(bool shouldSucceed);
}

public static class PromisesUsage
{
    public static async Task Run()
    {
        await JSHost.ImportAsync("PromisesShim", "/PromisesShim.js");

        Stopwatch sw = new();
        sw.Start();

        await PromisesInterop.Wait2Seconds(); // Await Promise
        Console.WriteLine($"Waited {sw.Elapsed.TotalSeconds:#.0}s.");

        sw.Restart();
        string str =
            await PromisesInterop.WaitGetString(); // Await promise (string return)
        Console.WriteLine(
            $"Waited {sw.Elapsed.TotalSeconds:#.0}s for WaitGetString: '{str}'");

        sw.Restart();
        // Await promise with string return.
        DateTime date = await PromisesInterop.WaitGetDate();
        Console.WriteLine(
            $"Waited {sw.Elapsed.TotalSeconds:#.0}s for WaitGetDate: '{date}'");

        // Await a JS fetch.
        string responseText = await PromisesInterop.FetchCurrentUrl();
        Console.WriteLine($"responseText.Length: {responseText.Length}");

        sw.Restart();

        await PromisesInterop.AsyncFunction(); // Await an async JS method
        Console.WriteLine(
            $"Waited {sw.Elapsed.TotalSeconds:#.0}s for AsyncFunction.");

        try
        {
            // Handle a promise rejection. Await an async JS method.
            await PromisesInterop.ConditionalSuccess(shouldSucceed: false);
        }
        catch (JSException ex) // Catch JS exception
        {
            Console.WriteLine($"JS Exception Caught: '{ex.Message}'");
        }
    }
}
