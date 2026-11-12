using System.Collections.Concurrent;
using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;

namespace App.Host.Tests.Steps;

/// <summary>One record as a part wrote it: every attribute, the payload too.</summary>
public sealed record CapturedRecord(string Category, LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Attributes, Exception? Exception);

/// <summary>
/// A logger provider that keeps every record with all its attributes, so the
/// scenarios see everything an entry point writes - what the base provider
/// then drops is the BuildingBlocks scenarios' concern.
/// </summary>
public sealed class CapturedLogs : ILoggerProvider
{
    private readonly ConcurrentQueue<CapturedRecord> _records = new();

    public CapturedLogs() => Factory = LoggerFactory.Create(logging => logging.SetMinimumLevel(LogLevel.Trace).AddProvider(this));

    /// <summary>The factory to hand to the code under test.</summary>
    public ILoggerFactory Factory { get; }

    public IReadOnlyList<CapturedRecord> Records => [.. _records];

    public ILogger CreateLogger(string categoryName) => new Logger(categoryName, _records);

    public void Dispose()
    {
    }

    private sealed class Logger(string category, ConcurrentQueue<CapturedRecord> records) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var attributes = (state as IEnumerable<KeyValuePair<string, object?>> ?? [])
                .Where(pair => pair.Key != LogState.OriginalFormatKey)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            records.Enqueue(new CapturedRecord(category, logLevel, formatter(state, exception), attributes, exception));
        }
    }
}
