// Source: skill vp-logging-in-dotnet

using System.Globalization;
using Microsoft.Extensions.Logging;

namespace MaskedLogging;

/// <summary>
/// The logging variation point's own settings. They are read here, not in
/// AppConfig, so they come and go together with this project.
/// </summary>
/// <param name="Payload">LOG_PAYLOAD: <see cref="PayloadAlways"/> (default), <see cref="PayloadErrors"/>, or <see cref="PayloadOff"/>.</param>
/// <param name="MaxBytes">LOG_PAYLOAD_MAX_BYTES: the size a payload is cut to once written as JSON; default 4096, at least 64.</param>
/// <param name="Unmasked">LOG_PAYLOAD_UNMASKED: write every payload as it is, for debugging only; allowed only with LOG_LEVEL=debug, warned at start and once per request. Default false.</param>
public sealed record MaskedLogConfig(string Payload, int MaxBytes, bool Unmasked = false)
{
    /// <summary>Every record keeps its payload.</summary>
    public const string PayloadAlways = "always";

    /// <summary>Only a record with failed = true keeps it.</summary>
    public const string PayloadErrors = "errors";

    /// <summary>No record keeps it, as with the base provider.</summary>
    public const string PayloadOff = "off";

    /// <summary>The settings with nothing set.</summary>
    public static MaskedLogConfig Default { get; } = new(PayloadAlways, 4096);

    /// <summary>
    /// Reads the settings from the environment; an unusable value throws a
    /// FormatException naming its variable. <paramref name="level"/> is the
    /// application's LOG_LEVEL: masking is switched off only at debug level,
    /// so one variable forgotten in a deployment cannot write every secret.
    /// </summary>
    public static MaskedLogConfig Load(LogLevel level)
    {
        var config = Default;
        if (Environment.GetEnvironmentVariable("LOG_PAYLOAD") is { Length: > 0 } payload)
        {
            if (payload is not (PayloadAlways or PayloadErrors or PayloadOff))
            {
                throw new FormatException($"logging config: LOG_PAYLOAD \"{payload}\" is not always, errors, or off");
            }
            config = config with { Payload = payload };
        }
        if (Environment.GetEnvironmentVariable("LOG_PAYLOAD_MAX_BYTES") is { Length: > 0 } raw)
        {
            if (!int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var maxBytes) || maxBytes < 64)
            {
                throw new FormatException($"logging config: LOG_PAYLOAD_MAX_BYTES \"{raw}\" is not a number of 64 or more");
            }
            config = config with { MaxBytes = maxBytes };
        }
        if (Environment.GetEnvironmentVariable("LOG_PAYLOAD_UNMASKED") is { Length: > 0 } unmasked)
        {
            if (!bool.TryParse(unmasked, out var on))
            {
                throw new FormatException($"logging config: LOG_PAYLOAD_UNMASKED \"{unmasked}\" is not true or false");
            }
            if (on && level > LogLevel.Debug)
            {
                throw new FormatException($"logging config: LOG_PAYLOAD_UNMASKED is true, which needs LOG_LEVEL=debug, not {level}");
            }
            config = config with { Unmasked = on };
        }
        return config;
    }
}
