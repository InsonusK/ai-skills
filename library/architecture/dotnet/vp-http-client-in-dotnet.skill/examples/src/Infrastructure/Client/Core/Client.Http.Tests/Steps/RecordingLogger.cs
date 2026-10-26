using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;

namespace Client.Http.Tests.Steps;

/// <summary>One record: its level, its message, and every attribute as it was written, the payload too.</summary>
public sealed record Record(LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Attributes);

/// <summary>A logger that keeps every record with all its attributes, so the scenarios see everything the client writes.</summary>
public sealed class RecordingLogger : ILogger
{
    public List<Record> Records { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var attributes = (state as IEnumerable<KeyValuePair<string, object?>> ?? [])
            .Where(attribute => attribute.Key != LogState.OriginalFormatKey)
            .ToDictionary(attribute => attribute.Key, attribute => attribute.Value);
        Records.Add(new Record(logLevel, formatter(state, exception), attributes));
    }
}
