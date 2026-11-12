using Microsoft.Extensions.Logging;

namespace App.Host.Config;

/// <summary>
/// The base plateau's own settings, read from the environment. A variation
/// point or an entry point never adds a property here: each reads its settings
/// in its own folder, so removing it removes its settings too.
/// </summary>
/// <param name="LogLevel">LOG_LEVEL: debug, info (default), warn, or error - the names of every stack.</param>
public sealed record AppConfig(LogLevel LogLevel)
{
    /// <summary>Reads the settings; throws <see cref="FormatException"/> naming the variable it could not parse.</summary>
    public static AppConfig Load()
    {
        var raw = Environment.GetEnvironmentVariable("LOG_LEVEL");
        var level = string.IsNullOrEmpty(raw) ? "info" : raw.ToLowerInvariant();
        return new AppConfig(level switch
        {
            "debug" => LogLevel.Debug,
            "info" => LogLevel.Information,
            "warn" => LogLevel.Warning,
            "error" => LogLevel.Error,
            _ => throw new FormatException($"config: LOG_LEVEL: unknown level \"{raw}\"; use debug, info, warn, or error"),
        });
    }
}
