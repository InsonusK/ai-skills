// Source: skill base-plateau-in-dotnet

using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Logging;

/// <summary>
/// The one way the application writes logs. Every part that logs - the command
/// behavior, an entry point, later a client - writes the same records through
/// <see cref="ILogger"/> with <see cref="LoggerWriteExtensions.Write(ILogger, LogLevel, string, KeyValuePair{string, object?}[])"/>
/// and the attributes below; which of them reach the output is decided only by
/// the logger provider the composition root adds:
/// <list type="bullet">
/// <item><see cref="JsonLogProvider"/>, the base plateau's: one JSON line per
/// record, the payload attribute dropped - who, what, how long, how it ended;
/// never the data;</item>
/// <item>the logging variation point's: the payload too, masked, with the
/// request id of the call.</item>
/// </list>
/// So a part never knows which provider runs, and attaching the logging
/// variation point changes what is written, not the code that writes it.
/// </summary>
public static class LogAttr
{
    /// <summary>
    /// The key of the data of a call: a command's parameters, a request's
    /// query, headers, and body, a call's arguments. The base provider drops
    /// it, so no data - and no secret in it - reaches the output unless the
    /// logging variation point masks it.
    /// </summary>
    public const string PayloadKey = "payload";

    /// <summary>
    /// The key that is true on a record of a call that ended with an error, so
    /// a provider can keep the payload of failed calls only.
    /// </summary>
    public const string FailedKey = "failed";

    /// <summary>An attribute of a record: <c>LogAttr.Of("command", "PlanItems")</c>.</summary>
    public static KeyValuePair<string, object?> Of(string key, object? value) => new(key, value);

    /// <summary>The payload attribute: <paramref name="value"/> is the call's data as it is - a command, a result, a map of a request's parts.</summary>
    public static KeyValuePair<string, object?> Payload(object? value) => new(PayloadKey, value);

    /// <summary>The failed attribute of the record that ends a call.</summary>
    public static KeyValuePair<string, object?> Failed(bool failed) => new(FailedKey, failed);
}

/// <summary>Writes a record of a fixed message and its attributes through any <see cref="ILogger"/>.</summary>
public static class LoggerWriteExtensions
{
    /// <summary>Writes a record: <paramref name="message"/> is fixed text ("command called"), every value goes into an attribute.</summary>
    public static void Write(this ILogger logger, LogLevel level, string message, params KeyValuePair<string, object?>[] attributes) =>
        logger.Write(level, message, null, attributes);

    /// <summary>Writes a record with the exception that ended the call.</summary>
    public static void Write(this ILogger logger, LogLevel level, string message, Exception? exception, params KeyValuePair<string, object?>[] attributes)
    {
        if (!logger.IsEnabled(level))
        {
            return;
        }
        logger.Log(level, default, new LogState(message, attributes), exception, static (state, _) => state.Message);
    }
}

/// <summary>
/// The state of a record as Microsoft.Extensions.Logging passes it to every
/// provider: the attributes in order, then "{OriginalFormat}" - the message -
/// which other providers (the console, a test logger) read as the template.
/// </summary>
public sealed class LogState(string message, IReadOnlyList<KeyValuePair<string, object?>> attributes)
    : IReadOnlyList<KeyValuePair<string, object?>>
{
    /// <summary>The key under which Microsoft.Extensions.Logging carries the message template.</summary>
    public const string OriginalFormatKey = "{OriginalFormat}";

    /// <summary>The fixed text of the record.</summary>
    public string Message { get; } = message;

    public int Count => attributes.Count + 1;

    public KeyValuePair<string, object?> this[int index] =>
        index == attributes.Count ? new(OriginalFormatKey, Message) : attributes[index];

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
        {
            yield return this[i];
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => Message;
}
