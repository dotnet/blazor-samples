using Microsoft.AspNetCore.Components;

namespace BlazorSample.Components.Pages;

public partial class CallJs7 : 
    ComponentBase, IObservable<ElementReference>, IDisposable
{
    private bool disposing;
    private readonly List<IObserver<ElementReference>> subscriptions = [];
    private ElementReference title;

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        foreach (var subscription in subscriptions)
        {
            try
            {
                subscription.OnNext(title);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public void Dispose()
    {
        disposing = true;

        foreach (var subscription in subscriptions)
        {
            try
            {
                subscription.OnCompleted();
            }
            catch (Exception)
            {
            }
        }

        subscriptions.Clear();

        // The following prevents derived types that introduce a
        // finalizer from needing to re-implement IDisposable.
        GC.SuppressFinalize(this);
    }

    public IDisposable Subscribe(IObserver<ElementReference> observer)
    {
        if (disposing)
        {
            throw new InvalidOperationException("Parent being disposed");
        }

        subscriptions.Add(observer);

        return new Subscription(observer, this);
    }

    private class Subscription(IObserver<ElementReference> observer,
        CallJs7 self) : IDisposable
    {
        public IObserver<ElementReference> Observer { get; } = observer;
        public CallJs7 Self { get; } = self;

        public void Dispose() => Self.subscriptions.Remove(Observer);
    }
}
