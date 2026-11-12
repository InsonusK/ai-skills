using System.Net;
using System.Net.Sockets;

namespace App.Host.Tests.Steps;

/// <summary>Ports for the scenarios that start a real server.</summary>
public static class FreePort
{
    /// <summary>A port nothing listens on right now.</summary>
    public static int Next()
    {
        using var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        return ((IPEndPoint)probe.LocalEndpoint).Port;
    }

    /// <summary>Holds <paramref name="port"/> on every address, as a server already serving there would.</summary>
    public static TcpListener Take(int port)
    {
        var holder = new TcpListener(IPAddress.IPv6Any, port);
        holder.Server.DualMode = true;
        holder.Start();
        return holder;
    }

    /// <summary>Waits until something accepts connections on <paramref name="port"/>.</summary>
    public static async Task WaitServingAsync(int port)
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(IPAddress.Loopback, port);
                return;
            }
            catch (SocketException)
            {
                await Task.Delay(50);
            }
        }
        throw new TimeoutException($"nothing serves on port {port}");
    }
}
