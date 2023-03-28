using System.Threading.Tasks;
using Microsoft.JSInterop;
using System;

public class JsInteropClasses1 : IDisposable
{
    private readonly IJSRuntime js;

    public JsInteropClasses1(IJSRuntime js)
    {
        this.js = js;
    }

    public async ValueTask TickerChanged(string symbol, decimal price)
    {
        await js.InvokeVoidAsync("displayTickerAlert1", symbol, price);
    }

    public void Dispose()
    {
    }
}
