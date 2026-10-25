// Source: skill base-plateau-in-dotnet

using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace BuildingBlocks.Logging;

/// <summary>
/// One record as one JSON line on <paramref name="output"/>: <c>time</c>,
/// <c>level</c>, <c>msg</c>, then the attributes as given, then the error. The
/// fields are the same as the Go base plateau's, so one log pipeline reads
/// every stack. Every provider writes through it - the base plateau's
/// <see cref="JsonLogProvider"/>, the logging variation point's - so the
/// fields stay the same whichever is attached; each provider decides which
/// attributes it gives.
/// </summary>
public sealed class JsonLogLine(TextWriter output)
{
    private static readonly JsonSerializerOptions ValueOptions = new(JsonSerializerOptions.Web)
    {
        // Log lines are read by people and log pipelines, not by a browser.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    // One record is one line, even when several threads log at once.
    private readonly Lock _writing = new();

    /// <summary>
    /// Writes the record; <see cref="LogState.OriginalFormatKey"/> among the
    /// <paramref name="attributes"/> is skipped, every other one is written.
    /// </summary>
    public void Write(LogLevel level, string message, IEnumerable<KeyValuePair<string, object?>> attributes, Exception? exception)
    {
        using var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer, new JsonWriterOptions { Encoder = ValueOptions.Encoder }))
        {
            json.WriteStartObject();
            json.WriteString("time", DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"));
            json.WriteString("level", LevelName(level));
            json.WriteString("msg", message);
            foreach (var (key, value) in attributes)
            {
                if (key == LogState.OriginalFormatKey)
                {
                    continue;
                }
                json.WritePropertyName(key);
                WriteValue(json, value);
            }
            if (exception is not null)
            {
                json.WriteString("error", exception.Message);
                // A failure of the service is debugged from its stack; a
                // rejected request (an error with a kind) needs none.
                if (ErrorKindException.KindOf(exception) is null)
                {
                    json.WriteString("exception", exception.ToString());
                }
            }
            json.WriteEndObject();
        }
        var line = System.Text.Encoding.UTF8.GetString(buffer.ToArray());
        lock (_writing)
        {
            output.WriteLine(line);
            output.Flush();
        }
    }

    // The level names of the Go base plateau (log/slog).
    private static string LevelName(LogLevel level) => level switch
    {
        LogLevel.Trace or LogLevel.Debug => "DEBUG",
        LogLevel.Information => "INFO",
        LogLevel.Warning => "WARN",
        _ => "ERROR",
    };

    private static void WriteValue(Utf8JsonWriter json, object? value)
    {
        switch (value)
        {
            case TimeSpan duration:
                // Nanoseconds, as the Go base plateau writes a duration.
                json.WriteNumberValue(duration.Ticks * 100);
                break;
            case Exception error:
                json.WriteStringValue(error.Message);
                break;
            default:
                try
                {
                    JsonSerializer.Serialize(json, value, value?.GetType() ?? typeof(object), ValueOptions);
                }
                catch (Exception)
                {
                    // A value that cannot be serialized never loses the record.
                    json.WriteStringValue(value?.ToString());
                }
                break;
        }
    }
}
