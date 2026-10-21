using System.Text.Json;
using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Tests.Steps;

/// <summary>
/// A logger that keeps every record with all its attributes, the payload too,
/// so the scenarios see everything a part writes - what the base provider then
/// drops is the provider's scenarios' concern.
/// </summary>
public sealed class RecordingLogger<TCategory> : ILogger<TCategory>
{
    public List<Dictionary<string, string>> Records { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var record = new Dictionary<string, string> { ["level"] = logLevel.ToString(), ["msg"] = formatter(state, exception) };
        foreach (var (key, value) in state as IEnumerable<KeyValuePair<string, object?>> ?? [])
        {
            if (key != LogState.OriginalFormatKey)
            {
                record[key] = value switch
                {
                    string text => text,
                    bool flag => flag.ToString(),
                    _ => JsonSerializer.Serialize(value, JsonSerializerOptions.Web),
                };
            }
        }
        if (exception is not null)
        {
            record["error"] = exception.Message;
        }
        Records.Add(record);
    }
}
