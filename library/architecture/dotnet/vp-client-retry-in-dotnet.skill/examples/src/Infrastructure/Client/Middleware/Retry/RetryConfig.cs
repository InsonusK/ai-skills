// Source: skill vp-client-retry-in-dotnet

using System.Globalization;
using System.Text.RegularExpressions;

namespace Retry;

/// <summary>The retry settings of one API, beside the {API} group of skill api-client.</summary>
/// <param name="Attempts">{API}_RETRY_ATTEMPTS: the attempts a call gets in all, the first included; 1 sends no repeat. Default 3.</param>
/// <param name="Backoff">{API}_RETRY_BACKOFF: the wait before the second attempt, doubled before each next one, each wait between half and one and a half of it. Default 100ms.</param>
public sealed partial record RetryConfig(int Attempts, TimeSpan Backoff)
{
    public const int DefaultAttempts = 3;

    public static readonly TimeSpan DefaultBackoff = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Reads {prefix}_RETRY_ATTEMPTS and {prefix}_RETRY_BACKOFF; throws
    /// <see cref="FormatException"/> naming the variable that is unusable.
    /// </summary>
    public static RetryConfig Load(string prefix)
    {
        var name = prefix.Replace('_', '-').ToLowerInvariant();
        var attempts = DefaultAttempts;
        if (Environment.GetEnvironmentVariable(prefix + "_RETRY_ATTEMPTS") is { Length: > 0 } rawAttempts)
        {
            if (!int.TryParse(rawAttempts, NumberStyles.None, CultureInfo.InvariantCulture, out attempts) || attempts < 1)
            {
                throw new FormatException($"{name} config: {prefix}_RETRY_ATTEMPTS \"{rawAttempts}\" is not a count of 1 or more");
            }
        }
        var backoff = DefaultBackoff;
        if (Environment.GetEnvironmentVariable(prefix + "_RETRY_BACKOFF") is { Length: > 0 } rawBackoff)
        {
            backoff = ParseDuration(rawBackoff) ?? throw new FormatException($"{name} config: {prefix}_RETRY_BACKOFF \"{rawBackoff}\" is not a duration like 100ms or 1s");
        }
        return new RetryConfig(attempts, backoff);
    }

    /// <summary>A number and ms, s, or m - the form every stack reads - or null.</summary>
    private static TimeSpan? ParseDuration(string raw)
    {
        var match = DurationPattern().Match(raw);
        if (!match.Success)
        {
            return null;
        }
        var number = double.Parse(match.Groups["number"].Value, CultureInfo.InvariantCulture);
        var duration = match.Groups["unit"].Value switch
        {
            "ms" => TimeSpan.FromMilliseconds(number),
            "s" => TimeSpan.FromSeconds(number),
            _ => TimeSpan.FromMinutes(number),
        };
        return duration > TimeSpan.Zero ? duration : null;
    }

    [GeneratedRegex(@"^(?<number>[0-9]+(\.[0-9]+)?)(?<unit>ms|s|m)$")]
    private static partial Regex DurationPattern();
}
