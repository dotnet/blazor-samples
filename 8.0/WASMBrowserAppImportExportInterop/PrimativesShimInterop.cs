using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public partial class PrimitivesInterop
{
    // Importing an existing JS method.
    [JSImport("globalThis.console.log")]
    public static partial void ConsoleLog([JSMarshalAs<JSType.Any>] object value);

    // Importing static methods from a JS module.
    [JSImport("incrementCounter", "PrimitivesShim")]
    public static partial void IncrementCounter();

    [JSImport("getCounter", "PrimitivesShim")]
    public static partial int GetCounter();

    // The JS shim method name isn't required to match the C# method name.
    [JSImport("logValue", "PrimitivesShim")]
    public static partial void LogInt(int value);

    // A second mapping to the same JS method with compatible type.
    [JSImport("logValue", "PrimitivesShim")]
    public static partial void LogString(string value);

    // Accept any type as parameter. .NET types are mapped to JS types where 
    // possible. Otherwise, they're marshalled as an untyped object reference 
    // to the .NET object proxy. The JS implementation logs to browser console 
    // the JS type and value to demonstrate results of marshalling.
    [JSImport("logValueAndType", "PrimitivesShim")]
    public static partial void LogValueAndType(
        [JSMarshalAs<JSType.Any>] object value);

    // Some types have multiple mappings and require explicit marshalling to the 
    // desired JS type. A long/Int64 can be mapped as either a Number or BigInt.
    // Passing a long value to the above method generates an error at runtime:
    // "ToJS for System.Int64 is not implemented." ("ToJS" means "to JavaScript")
    // If the parameter declaration `Method(JSMarshalAs<JSType.Any>] long value)` 
    // is used, a compile-time error is generated:
    // "Type long is not supported by source-generated JS interop...."
    // Instead, explicitly map the long parameter to either a JSType.Number or 
    // JSType.BigInt. Note that runtime overflow errors are possible in JS if the 
    // C# value is too large.
    [JSImport("logValueAndType", "PrimitivesShim")]
    public static partial void LogValueAndTypeForNumber(
        [JSMarshalAs<JSType.Number>] long value);

    [JSImport("logValueAndType", "PrimitivesShim")]
    public static partial void LogValueAndTypeForBigInt(
        [JSMarshalAs<JSType.BigInt>] long value);
}

public static class PrimitivesUsage
{
    public static async Task Run()
    {
        // Ensure JS module loaded.
        await JSHost.ImportAsync("PrimitivesShim", "/PrimitivesShim.js");

        // Call a proxy to a static JS method, console.log().
        PrimitivesInterop.ConsoleLog("Printed from JSImport of console.log()");

        // Basic examples of JS interop with an integer.
        PrimitivesInterop.IncrementCounter();
        int counterValue = PrimitivesInterop.GetCounter();
        PrimitivesInterop.LogInt(counterValue);
        PrimitivesInterop.LogString("I'm a string from .NET in your browser!");

        // Mapping some other .NET types to JS primitives.
        PrimitivesInterop.LogValueAndType(true);
        PrimitivesInterop.LogValueAndType(0x3A); // Byte literal
        PrimitivesInterop.LogValueAndType('C');
        PrimitivesInterop.LogValueAndType((Int16)12);
        // JS Number has a lower max value and can generate overflow errors.
        PrimitivesInterop.LogValueAndTypeForNumber(9007199254740990L); // Int64/Long
        // Next line: Int64/Long, JS BigInt supports larger numbers.
        PrimitivesInterop.LogValueAndTypeForBigInt(1234567890123456789L);// 
        PrimitivesInterop.LogValueAndType(3.14f); // Single floating point literal
        PrimitivesInterop.LogValueAndType(3.14d); // Double floating point literal
        PrimitivesInterop.LogValueAndType("A string");
    }
}
