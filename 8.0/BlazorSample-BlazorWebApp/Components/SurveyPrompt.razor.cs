using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorSample.Components;

public partial class SurveyPrompt : 
    ComponentBase, IObserver<ElementReference>, IDisposable
{
    private IDisposable? subscription = null;

    [Parameter]
    public IObservable<ElementReference>? Parent { get; set; }

    [Inject]
    public IJSRuntime? JS {get; set;}

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        subscription?.Dispose();
        subscription = Parent?.Subscribe(this);
    }

    public void OnCompleted()
    {
        subscription = null;
    }

    public void OnError(Exception error)
    {
        subscription = null;
    }

    public void  OnNext(ElementReference value)
    {
        _ = (JS?.InvokeAsync<object>(
            "setElementClass", [value, "red"]));
    }

    public void Dispose()
    {
        subscription?.Dispose();

        // The following prevents derived types that introduce a
        // finalizer from needing to re-implement IDisposable.
        GC.SuppressFinalize(this);
    }
}
