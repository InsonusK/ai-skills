// Source: skill vp-logging-in-dotnet

using System.Collections.Concurrent;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;

namespace MaskedLogging;

/// <summary>
/// The logging variation point's provider: one JSON line per record, like the
/// base <see cref="JsonLogProvider"/>, but keeping the payload - masked
/// (<see cref="PayloadMask"/>), cut to <see cref="MaskedLogConfig.MaxBytes"/>,
/// and only on the records <see cref="MaskedLogConfig.Payload"/> allows. It
/// adds the request id of the call (<see cref="RequestId"/>), and warns once
/// about every place a body could not be read. The composition root adds it in
/// place of <see cref="JsonLogProvider"/>; no part that logs changes.
/// </summary>
/// <param name="output">Where the lines go: standard error.</param>
/// <param name="minimum">The level the logging builder filters at; a warning this provider adds is written only when that level lets it.</param>
/// <param name="config">The payload rules.</param>
public sealed class MaskedLogProvider : ILoggerProvider
{
    /// <summary>The attribute the request id of the call is written under.</summary>
    public const string RequestIdKey = "request_id";

    /// <summary>
    /// The WARN record written the first time a record of one message holds a
    /// body the mask could not read in one place of its payload.
    /// </summary>
    public const string NotParsedMessage = "log payload not parsed";

    /// <summary>
    /// The WARN record written when the provider is built with
    /// <see cref="MaskedLogConfig.Unmasked"/>, and then once per request -
    /// before its first record with a payload - and before every such record
    /// outside a request.
    /// </summary>
    public const string UnmaskedMessage = "log payload unmasked";

    // How many request ids are remembered as warned about unmasking; an older
    // request may be warned about again.
    private const int RecentRequests = 4096;

    private static readonly JsonSerializerOptions PayloadOptions = new()
    {
        // "<" and ">" as they are, so the size a payload is cut at is the size every stack counts.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly JsonLogLine _line;
    private readonly LogLevel _minimum;
    private readonly MaskedLogConfig _config;

    // "message\0path" of every unparsed body already warned about.
    private readonly ConcurrentDictionary<string, byte> _warned = new();

    // The request ids already warned about unmasking, oldest first.
    private readonly HashSet<string> _unmaskedRequests = [];
    private readonly Queue<string> _unmaskedOrder = new();
    private readonly Lock _unmasking = new();

    public MaskedLogProvider(TextWriter output, LogLevel minimum, MaskedLogConfig config)
    {
        _line = new JsonLogLine(output);
        _minimum = minimum;
        _config = config;
        if (config.Unmasked)
        {
            _line.Write(LogLevel.Warning, UnmaskedMessage, [], null); // at once: masking is off
        }
    }

    public ILogger CreateLogger(string categoryName) => new MaskedLogger(this);

    public void Dispose()
    {
    }

    private bool IsEnabled(LogLevel level) => level != LogLevel.None && level >= _minimum;

    // Writes the record with its payload rendered, or without it, then a
    // warning for each body of it not yet warned about.
    private void Write<TState>(LogLevel level, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var given = state as IEnumerable<KeyValuePair<string, object?>> ?? [];
        var failed = given.Any(a => a.Key == LogAttr.FailedKey && a.Value is true);
        var attributes = new List<KeyValuePair<string, object?>>();
        if (RequestId.Current is { } id)
        {
            attributes.Add(LogAttr.Of(RequestIdKey, id));
        }
        var unparsed = new List<Unparsed>();
        string? template = null;
        var hasPayload = false;
        foreach (var (key, value) in given)
        {
            if (key == LogState.OriginalFormatKey)
            {
                template = value as string;
            }
            else if (key != LogAttr.PayloadKey)
            {
                attributes.Add(new(key, value));
            }
            else
            {
                hasPayload = true;
                if (Keeps(failed))
                {
                    attributes.Add(LogAttr.Payload(Payload(value, unparsed)));
                    if (_config.Unmasked)
                    {
                        WarnUnmasked();
                    }
                }
            }
        }
        // A message template (logger.LogInformation("... {payload}", x))
        // renders its values into the text; with a payload among them the text
        // would carry it unmasked, so the template is written instead.
        var message = hasPayload && state is not LogState && template is not null ? template : formatter(state, exception);
        _line.Write(level, message, attributes, exception);
        Warn(message, unparsed);
    }

    // Whether a record of this outcome keeps its payload.
    private bool Keeps(bool failed) => _config.Payload switch
    {
        MaskedLogConfig.PayloadOff => false,
        MaskedLogConfig.PayloadErrors => failed,
        _ => true,
    };

    // The payload masked and, past MaxBytes, cut: then it is the beginning of
    // its JSON as a text, saying how long the whole was. The mask runs before
    // the cut, so a cut never leaves a secret unmasked.
    private object? Payload(object? value, List<Unparsed> unparsed)
    {
        MaskedPayload masked;
        try
        {
            masked = _config.Unmasked ? new MaskedPayload(PayloadMask.Unmask(value), []) : PayloadMask.Apply(value);
        }
        catch (Exception error)
        {
            return $"payload not written: {error.Message}";
        }
        unparsed.AddRange(masked.Unparsed);
        if (masked.Value is null)
        {
            return null;
        }
        var encoded = Encoding.UTF8.GetBytes(masked.Value.ToJsonString(PayloadOptions));
        if (encoded.Length > _config.MaxBytes)
        {
            return $"{Encoding.UTF8.GetString(encoded, 0, _config.MaxBytes)}... (cut, {encoded.Length} bytes)";
        }
        return masked.Value;
    }

    // A WARN record before the first record of a request that carries an
    // unmasked payload, and before every such record outside a request (with
    // no request id), so nobody reads an unmasked log unwarned.
    private void WarnUnmasked()
    {
        var id = RequestId.Current;
        if (id is not null)
        {
            lock (_unmasking)
            {
                if (!_unmaskedRequests.Add(id))
                {
                    return;
                }
                _unmaskedOrder.Enqueue(id);
                if (_unmaskedOrder.Count > RecentRequests)
                {
                    _unmaskedRequests.Remove(_unmaskedOrder.Dequeue());
                }
            }
        }
        List<KeyValuePair<string, object?>> attributes = id is null ? [] : [LogAttr.Of(RequestIdKey, id)];
        _line.Write(LogLevel.Warning, UnmaskedMessage, attributes, null);
    }

    // A WARN record for each unparsed body the first time a record of this
    // message has one at that path: a body nobody can read says so once, not
    // on every request, and the record keeps the level its code chose.
    private void Warn(string message, List<Unparsed> unparsed)
    {
        if (!IsEnabled(LogLevel.Warning))
        {
            return;
        }
        foreach (var body in unparsed)
        {
            if (!_warned.TryAdd($"{message}\0{body.Path}", 0))
            {
                continue;
            }
            List<KeyValuePair<string, object?>> attributes = [];
            if (RequestId.Current is { } id)
            {
                attributes.Add(LogAttr.Of(RequestIdKey, id));
            }
            attributes.AddRange([LogAttr.Of("record", message), LogAttr.Of("field", body.Path), LogAttr.Of("bytes", body.Bytes)]);
            _line.Write(LogLevel.Warning, NotParsedMessage, attributes, null);
        }
    }

    private sealed class MaskedLogger(MaskedLogProvider provider) : ILogger
    {
        // A scope is not written, so a payload given to one never reaches the log.
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => provider.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                provider.Write(logLevel, state, exception, formatter);
            }
        }
    }
}
