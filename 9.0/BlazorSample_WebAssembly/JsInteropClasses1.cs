using Microsoft.JSInterop;

namespace BlazorSample;

public class JsInteropClasses1(IJSRuntime js) : IDisposable
{
    private readonly IJSRuntime js = js;

    public async ValueTask TickerChanged(string symbol, decimal price) => 
        await js.InvokeVoidAsync("displayTickerAlert1", symbol, price);

    // Calling SuppressFinalize(this) prevents derived types that introduce 
    // a finalizer from needing to re-implement IDisposable.
    public void Dispose() => GC.SuppressFinalize(this);
}
