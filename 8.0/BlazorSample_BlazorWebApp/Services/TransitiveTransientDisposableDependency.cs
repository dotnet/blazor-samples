namespace BlazorSample.Services;

public class TransitiveTransientDisposableDependency
    : ITransitiveTransientDisposableDependency, IDisposable
{
    public void Dispose() => GC.SuppressFinalize(this);
}

public interface ITransitiveTransientDisposableDependency
{
}

public class TransientDependency(ITransitiveTransientDisposableDependency
        transitiveTransientDisposableDependency)
{
}
