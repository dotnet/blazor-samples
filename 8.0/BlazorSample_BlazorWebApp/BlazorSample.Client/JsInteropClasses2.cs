using Microsoft.JSInterop;

public class JsInteropClasses2 : IDisposable
{
    private readonly IJSRuntime js;

    public JsInteropClasses2(IJSRuntime js)
    {
        this.js = js;
    }

    public async ValueTask<string> TickerChanged(string symbol, decimal price)
    {
        return await js.InvokeAsync<string>("displayTickerAlert2", symbol, price);
    }

    public void Dispose()
    {
    }
}
