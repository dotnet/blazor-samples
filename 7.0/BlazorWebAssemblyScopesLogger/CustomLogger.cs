using System.Diagnostics.CodeAnalysis;
using System.Text;
using static CustomLoggerConfiguration;

public sealed class CustomLogger : ILogger
{
    private readonly string name;
    private readonly Func<CustomLoggerConfiguration> getCurrentConfig;
    private readonly CustomLoggerProvider provider;

    public List<LogScopeInfo>? Scopes { get; set; }

    public CustomLogger(
        CustomLoggerProvider provider,
        string name,
        Func<CustomLoggerConfiguration> getCurrentConfig) =>
        (this.provider, this.name, this.getCurrentConfig) = (provider, name, getCurrentConfig);

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return provider.ScopeProvider.Push(state);
    }

    public bool IsEnabled(LogLevel logLevel) =>
        getCurrentConfig().LogLevels.ContainsKey(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var scopes = new StringBuilder();

        if (!IsEnabled(logLevel))
        {
            return;
        }

        CustomLoggerConfiguration config = getCurrentConfig();

        if (config.EventId == 0 || config.EventId == eventId.Id)
        {
            scopes.Clear();
            var scopeProvider = provider.ScopeProvider;

            if (scopeProvider != null)
            {
                scopeProvider.ForEachScope((scope, stringBuilder) =>
                {
                    stringBuilder.Append(" => ").Append(scope);
                }, scopes);
            }

            switch (config.LogLevels[logLevel])
            {
                case LogFormat.Short:
                    Console.WriteLine($"{name}: {formatter(state, exception)}{scopes}");
                    break;
                case LogFormat.Long:
                    Console.WriteLine($"[{eventId.Id, 2}: {logLevel, -12}] {name} - {formatter(state, exception)}{scopes}");
                    break;
                default:
                    // No-op
                    break;
            }
        }
    }
}
