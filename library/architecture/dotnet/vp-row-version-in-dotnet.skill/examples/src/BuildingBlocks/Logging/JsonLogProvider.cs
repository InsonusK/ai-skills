// Source: skill base-plateau-in-dotnet

using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Logging;

/// <summary>
/// The base plateau's logger provider: one JSON line per record on
/// <paramref name="output"/> (<see cref="JsonLogLine"/>) with every payload
/// attribute dropped. The level is filtered by the logging builder
/// (<c>SetMinimumLevel</c>), not here.
/// </summary>
public sealed class JsonLogProvider(TextWriter output) : ILoggerProvider
{
    private readonly JsonLogLine _line = new(output);

    public ILogger CreateLogger(string categoryName) => new JsonLogger(_line);

    public void Dispose()
    {
    }

    private sealed class JsonLogger(JsonLogLine line) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            var attributes = state as IEnumerable<KeyValuePair<string, object?>> ?? [];
            // the payload never reaches the base output
            line.Write(logLevel, formatter(state, exception), attributes.Where(a => a.Key != LogAttr.PayloadKey), exception);
        }
    }
}
