namespace BlazorSample.Services;

public class TransientDisposableService : IDisposable
{
    public void Dispose() => GC.SuppressFinalize(this);
}
