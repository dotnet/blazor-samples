using Microsoft.JSInterop;

namespace BlazorSample;

public class JsInteropClasses3(IJSRuntime js)
{
    private readonly IJSRuntime js = js;

    public async ValueTask<string> CallHelloHelperGetHelloMessage(string? name)
    {
        using var objRef = DotNetObjectReference.Create(new HelloHelper(name));
        return await js.InvokeAsync<string>("sayHello1", objRef);
    }
}
