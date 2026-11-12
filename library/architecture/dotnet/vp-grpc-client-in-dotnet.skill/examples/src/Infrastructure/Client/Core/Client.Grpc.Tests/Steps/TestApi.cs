using System.Net;
using System.Net.Sockets;
using Client.Grpc.Tests.Echo;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Client.Grpc.Tests.Steps;

/// <summary>What the test API was sent.</summary>
public sealed record Received(string Text, string? RequestId);

/// <summary>
/// The test API's EchoService as a scenario sets it: it answers its text or
/// fails with its status, after its delay, and records what it received.
/// </summary>
public sealed class EchoBehavior : EchoService.EchoServiceBase
{
    public string Text { get; set; } = "";
    public RpcException? Failure { get; set; }
    public TimeSpan Delay { get; set; }
    public List<Received> Received { get; } = [];

    public override async Task<EchoMessage> Echo(EchoMessage request, ServerCallContext context)
    {
        Received.Add(new Received(request.Text, context.RequestHeaders.GetValue(CallInterceptor.RequestIdMetadata)));
        try
        {
            await Task.Delay(Delay, context.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            // the client gave up waiting
        }
        return Failure is null ? new EchoMessage { Text = Text } : throw Failure;
    }
}

/// <summary>
/// The test API: a gRPC server of these scenarios on Kestrel, HTTP/2 in plain
/// text on a free port of 127.0.0.1, serving <see cref="EchoBehavior"/>.
/// </summary>
public sealed class TestApi : IAsyncDisposable
{
    private WebApplication? _app;

    public EchoBehavior Echo { get; } = new();
    public int Port { get; } = FreePort();

    /// <summary>Starts serving; a scenario whose API is not listening never calls it.</summary>
    public async Task StartAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(kestrel => kestrel.Listen(IPAddress.Loopback, Port, listen => listen.Protocols = HttpProtocols.Http2));
        builder.Services.AddGrpc();
        builder.Services.AddSingleton(Echo);
        _app = builder.Build();
        _app.MapGrpcService<EchoBehavior>();
        await _app.StartAsync();
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

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }
}
