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
        return js.InvokeAsync<string>("sayHello3", 
            DotNetObjectReference.Create(new HelloHelper(name)));
    }
}
