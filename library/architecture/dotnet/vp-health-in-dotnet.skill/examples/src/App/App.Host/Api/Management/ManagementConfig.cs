// Source: skill vp-health-in-dotnet

namespace App.Host.Api.Management;

/// <summary>
/// The management entry point's own settings. They are read here, not in
/// App.Host/Config, so they come and go together with this folder.
/// </summary>
/// <param name="ListenPort">MANAGEMENT_LISTEN_PORT, the port the probes call; default 8081.</param>
public sealed record ManagementConfig(int ListenPort)
{
    /// <summary>Reads the settings; throws <see cref="FormatException"/> naming the variable it could not parse.</summary>
    public static ManagementConfig Load()
    {
        var raw = Environment.GetEnvironmentVariable("MANAGEMENT_LISTEN_PORT");
        if (string.IsNullOrEmpty(raw))
        {
            return new ManagementConfig(8081);
        }
        if (!ushort.TryParse(raw, out var port) || port == 0)
        {
            throw new FormatException($"management config: MANAGEMENT_LISTEN_PORT \"{raw}\" is not a port");
        }
        return new ManagementConfig(port);
    }
}
