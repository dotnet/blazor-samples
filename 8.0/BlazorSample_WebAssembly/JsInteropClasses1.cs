using Microsoft.JSInterop;

namespace BlazorSample;

public class JsInteropClasses1(IJSRuntime js) : IDisposable
{
    private readonly IJSRuntime js = js;

    public async ValueTask TickerChanged(string symbol, decimal price)
    {
        await js.InvokeVoidAsync("displayTickerAlert1", symbol, price);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
