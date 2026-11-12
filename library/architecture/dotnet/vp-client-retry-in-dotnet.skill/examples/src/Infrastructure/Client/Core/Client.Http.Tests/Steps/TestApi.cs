using System.Net;
using System.Net.Sockets;
using System.Text;
using Client.Http;

namespace Client.Http.Tests.Steps;

/// <summary>What the test API was sent.</summary>
public sealed record Received(string Method, string Path, string Body, string? RequestId);

/// <summary>
/// The test API: an HTTP server of these scenarios only, on a free port of
/// 127.0.0.1, so the fixed code is tested without any API of the application.
/// It answers every request with the status and body a scenario sets, after
/// its delay, and records what it received.
/// </summary>
public sealed class TestApi : IDisposable
{
    private readonly HttpListener _listener = new();

    public int Status { get; set; } = 200;
    public string Body { get; set; } = "";
    public TimeSpan Delay { get; set; }
    public List<Received> Received { get; } = [];
    public int Port { get; } = FreePort();

    /// <summary>Starts answering; a scenario whose API is not listening never calls it.</summary>
    public void Start()
    {
        _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
        _listener.Start();
        _ = ServeAsync();
    }

    private async Task ServeAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception) when (!_listener.IsListening)
            {
                return;
            }
            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
            Received.Add(new Received(context.Request.HttpMethod, context.Request.Url!.AbsolutePath, await reader.ReadToEndAsync(),
                context.Request.Headers[HttpApiClient.RequestIdHeader]));
            await Task.Delay(Delay);
            try
            {
                context.Response.StatusCode = Status;
                context.Response.ContentType = "application/json";
                var bytes = Encoding.UTF8.GetBytes(Body);
                await context.Response.OutputStream.WriteAsync(bytes);
                context.Response.Close();
            }
            catch (Exception)
            {
                // the client gave up waiting
            }
        }
    }

    /// <summary>A port nothing listens on yet.</summary>
    private static int FreePort()
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        return port;
    }

    public void Dispose()
    {
        if (_listener.IsListening)
        {
            _listener.Stop();
        }
        _listener.Close();
    }
}
