namespace BlazorSample.Services;

public class TransitiveTransientDisposableDependency 
    : ITransitiveTransientDisposableDependency, IDisposable
{
    public void Dispose() => GC.SuppressFinalize(this);
}

public interface ITransitiveTransientDisposableDependency
{
}

#pragma warning disable CS9113
public class TransientDependency(ITransitiveTransientDisposableDependency
    transitiveTransientDisposableDependency)
{
}
#pragma warning restore CS9113
