using Microsoft.JSInterop;

namespace BlazorSample;

public class JsInteropClasses2(IJSRuntime js) : IDisposable
{
    private readonly IJSRuntime js = js;

    public async ValueTask<string> TickerChanged(string symbol, decimal price) => 
        await js.InvokeAsync<string>("displayTickerAlert2", symbol, price);

    // Calling SuppressFinalize(this) prevents derived types that introduce 
    // a finalizer from needing to re-implement IDisposable.
    public void Dispose() => GC.SuppressFinalize(this);
}
