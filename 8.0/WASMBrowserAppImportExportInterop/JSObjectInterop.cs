using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public partial class JSObjectInterop
{
    [JSImport("createObject", "JSObjectShim")]
    public static partial JSObject CreateObject();

    [JSImport("incrementAnswer", "JSObjectShim")]
    public static partial void IncrementAnswer(JSObject jsObject);

    [JSImport("summarize", "JSObjectShim")]
    public static partial string Summarize(JSObject jsObject);

    [JSImport("globalThis.console.log")]
    public static partial void ConsoleLog([JSMarshalAs<JSType.Any>] object value);
}

public static class JSObjectUsage
{
    public static async Task Run()
    {
        await JSHost.ImportAsync("JSObjectShim", "/JSObjectShim.js");

        JSObject jsObject = JSObjectInterop.CreateObject();
        JSObjectInterop.ConsoleLog(jsObject);
        JSObjectInterop.IncrementAnswer(jsObject);
        // An updated object isn't retrieved. The change is reflected in the 
        // existing instance.
        JSObjectInterop.ConsoleLog(jsObject);

        // JSObject exposes several methods for interacting with properties.
        jsObject.SetProperty("question", "What is the answer?");
        JSObjectInterop.ConsoleLog(jsObject);

        // We can't directly JSImport an instance method on the jsObject, but we 
        // can pass the object reference and have the JS shim call the instance 
        // method.
        string summary = JSObjectInterop.Summarize(jsObject);
        Console.WriteLine("Summary: " + summary);
    }
}
