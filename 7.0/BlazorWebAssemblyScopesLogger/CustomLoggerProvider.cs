using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

[ProviderAlias("CustomLog")]
public sealed class CustomLoggerProvider : IDisposable, ILoggerProvider, ISupportExternalScope
{
    private readonly IDisposable? onChangeToken;
    private CustomLoggerConfiguration config;
    private readonly ConcurrentDictionary<string, CustomLogger> loggers =
        new(StringComparer.OrdinalIgnoreCase);
    private IExternalScopeProvider scopeProvider = default!;

    void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        this.scopeProvider = scopeProvider;
    }

    public CustomLoggerProvider(
        IOptionsMonitor<CustomLoggerConfiguration> config)
    {
        this.config = config.CurrentValue;
        onChangeToken = config.OnChange(updatedConfig => this.config = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName) =>
        loggers.GetOrAdd(categoryName, name => new CustomLogger(this, name, GetCurrentConfig));

    private CustomLoggerConfiguration GetCurrentConfig() => config;

    internal IExternalScopeProvider ScopeProvider
    {
        get
        {
            if (scopeProvider is null)
            {
                scopeProvider = new LoggerExternalScopeProvider();
            }
                
            return scopeProvider;
        }
    }

    public void Dispose()
    {
        loggers.Clear();
        onChangeToken?.Dispose();
    }
}
