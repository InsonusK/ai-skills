// Source: skill vp-grpc-client-in-dotnet

using System.Globalization;
using System.Text.RegularExpressions;

namespace Client.Grpc;

/// <summary>
/// The settings of one gRPC API: the {API} group of skill api-client, read by
/// <see cref="Load"/> from the variables of the API's prefix.
/// </summary>
/// <param name="Name">How the log names the API: the prefix in lower case with "-", "TODO_API" is "todo-api".</param>
/// <param name="Host">{API}_HOST; <see cref="Mock"/> runs the client against its emulator.</param>
/// <param name="Port">{API}_GRPC_PORT; default 443 with TLS, 80 without.</param>
/// <param name="Tls">{API}_TLS: connect over TLS; default false.</param>
/// <param name="Timeout">{API}_TIMEOUT: the longest one call may take; default 5s.</param>
public sealed partial record GrpcApiConfig(string Name, string Host, int Port, bool Tls, TimeSpan Timeout)
{
    /// <summary>The value of {API}_HOST that runs the client against the emulator of its API inside the process.</summary>
    public const string Mock = "mock";

    /// <summary>Bounds a call when {API}_TIMEOUT is not set.</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Whether the client runs against its emulator.</summary>
    public bool Mocked => Host == Mock;

    /// <summary>The address the channel connects to: https://host:port with TLS, http://host:port (HTTP/2 in plain text) without.</summary>
    public Uri Target => new($"{(Tls ? "https" : "http")}://{Host}:{Port}");

    /// <summary>
    /// Reads the {API} group of <paramref name="prefix"/> ("TODO_API") from the
    /// environment; throws <see cref="FormatException"/> naming the variable
    /// that is missing or unusable.
    /// </summary>
    public static GrpcApiConfig Load(string prefix)
    {
        var name = prefix.Replace('_', '-').ToLowerInvariant();
        var host = Environment.GetEnvironmentVariable(prefix + "_HOST");
        if (string.IsNullOrEmpty(host))
        {
            throw new FormatException($"{name} config: {prefix}_HOST is not set");
        }
        var tls = false;
        if (Environment.GetEnvironmentVariable(prefix + "_TLS") is { Length: > 0 } rawTls && !bool.TryParse(rawTls, out tls))
        {
            throw new FormatException($"{name} config: {prefix}_TLS \"{rawTls}\" is not a bool");
        }
        var port = tls ? 443 : 80;
        if (Environment.GetEnvironmentVariable(prefix + "_GRPC_PORT") is { Length: > 0 } rawPort)
        {
            if (!ushort.TryParse(rawPort, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) || parsed == 0)
            {
                throw new FormatException($"{name} config: {prefix}_GRPC_PORT \"{rawPort}\" is not a port");
            }
            port = parsed;
        }
        var timeout = DefaultTimeout;
        if (Environment.GetEnvironmentVariable(prefix + "_TIMEOUT") is { Length: > 0 } rawTimeout)
        {
            timeout = ParseTimeout(rawTimeout) ?? throw new FormatException($"{name} config: {prefix}_TIMEOUT \"{rawTimeout}\" is not a duration like 5s or 500ms");
        }
        return new GrpcApiConfig(name, host, port, tls, timeout);
    }

    /// <summary>A number and ms, s, or m - the form every stack reads - or null.</summary>
    private static TimeSpan? ParseTimeout(string raw)
    {
        var match = TimeoutPattern().Match(raw);
        if (!match.Success)
        {
            return null;
        }
        var number = double.Parse(match.Groups["number"].Value, CultureInfo.InvariantCulture);
        var timeout = match.Groups["unit"].Value switch
        {
            "ms" => TimeSpan.FromMilliseconds(number),
            "s" => TimeSpan.FromSeconds(number),
            _ => TimeSpan.FromMinutes(number),
        };
        return timeout > TimeSpan.Zero ? timeout : null;
    }

    [GeneratedRegex(@"^(?<number>[0-9]+(\.[0-9]+)?)(?<unit>ms|s|m)$")]
    private static partial Regex TimeoutPattern();
}
