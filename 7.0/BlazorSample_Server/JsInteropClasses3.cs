using Microsoft.JSInterop;

public class JsInteropClasses3
{
    private readonly IJSRuntime js;

    public JsInteropClasses3(IJSRuntime js)
    {
        this.js = js;
    }

    public ValueTask<string> CallHelloHelperGetHelloMessage(string? name)
    {
        using var objRef = DotNetObjectReference.Create(new HelloHelper(name));
        return await js.InvokeAsync<string>("sayHello1", objRef);
    }
}
