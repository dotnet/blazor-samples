namespace BlazorSample.Services;

public class TransitiveTransientDisposableDependency
    : ITransitiveTransientDisposableDependency, IDisposable
{
    public void Dispose() => GC.SuppressFinalize(this);
}

public interface ITransitiveTransientDisposableDependency
{
}

public class TransientDependency
{
    private readonly ITransitiveTransientDisposableDependency
        transitiveTransientDisposableDependency;

    public TransientDependency(ITransitiveTransientDisposableDependency
        transitiveTransientDisposableDependency)
    {
        this.transitiveTransientDisposableDependency =
            transitiveTransientDisposableDependency;
    }
}
