using System.Text.Json;
using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;

namespace Middleware.Tests.Steps;

/// <summary>
/// Keeps every record as the JSON line <see cref="JsonLogLine"/> writes it,
/// payload included, read back into its fields: a text as it is, any other
/// value as its JSON. The scenarios so compare what every stack writes.
/// </summary>
public sealed class RecordingLogger<TCategory> : ILogger<TCategory>
{
    public List<Dictionary<string, string>> Records { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var written = new StringWriter();
        new JsonLogLine(written).Write(logLevel, formatter(state, exception), state as IEnumerable<KeyValuePair<string, object?>> ?? [], exception);
        var record = new Dictionary<string, string>();
        foreach (var field in JsonDocument.Parse(written.ToString()).RootElement.EnumerateObject())
        {
            record[field.Name] = field.Value.ValueKind == JsonValueKind.String ? field.Value.GetString()! : field.Value.GetRawText();
        }
        Records.Add(record);
    }
}
