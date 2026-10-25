using App.Host.Api.Grpc.Pipeline;
using App.Host.Tests.Steps.Grpc.Contract;
using BuildingBlocks.Pipeline;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace App.Host.Tests.Steps.Grpc;

/// <summary>A command of the scenarios, sent by the test method EchoService.Echo.</summary>
public sealed record GrpcEcho(string Text) : IRequest<string>;

/// <summary>The fake handler of <see cref="GrpcEcho"/>: answers what the scenario set, records what it received.</summary>
public sealed class FakeGrpcEcho : IRequestHandler<GrpcEcho, string>
{
    public string Answer { get; set; } = "";
    public Exception? Failure { get; set; }
    public TimeSpan Delay { get; set; }
    public string? Received { get; private set; }
    public string? ReceivedRequestId { get; private set; }

    public async Task<string> Handle(GrpcEcho command, CancellationToken cancellationToken)
    {
        Received = command.Text;
        ReceivedRequestId = BuildingBlocks.Logging.RequestId.Current;
        await Task.Delay(Delay, CancellationToken.None); // a call in flight finishes even when the server stops
        return Failure is null ? Answer : throw Failure;
    }
}

/// <summary>
/// The test service: an empty text cannot become the command, as a request a
/// method cannot decode, and is rejected as invalid.
/// </summary>
public sealed class EchoServiceImpl(ISender sender) : EchoService.EchoServiceBase
{
    public override async Task<EchoResponse> Echo(EchoRequest request, ServerCallContext context)
    {
        if (request.Text == "")
        {
            throw new ErrorKindException(ErrorKind.Invalid, "text is required");
        }
        return new EchoResponse { Text = await sender.Send(new GrpcEcho(request.Text), context.CancellationToken) };
    }
}

/// <summary>
/// The gRPC server of one scenario, built through the fixed pipeline
/// (<see cref="GrpcServer.Build"/>) on a test server: its settings, the fake
/// handlers its services send to, and the records it wrote. The services are
/// the test EchoService unless a scenario sets the application's own.
/// </summary>
public sealed class GrpcWorld : IDisposable
{
    private WebApplication? _app;
    private GrpcChannel? _channel;

    public GrpcConfig Config { get; set; } = new(9090, false);
    public CapturedLogs Logs { get; } = new();
    public FakeGrpcEcho Echo { get; } = new();
    /// <summary>The handlers the services reach, registered before the server is built.</summary>
    public ServiceCollection Handlers { get; } = new();
    /// <summary>The application's interceptors around every call, the first the outermost.</summary>
    public List<Interceptor> Interceptors { get; } = [];
    /// <summary>The services under test, given the sender to the fake handlers.</summary>
    public Func<ISender, IReadOnlyList<GrpcService>> Services { get; set; } = sender => [GrpcService.Of(new EchoServiceImpl(sender))];

    /// <summary>The sender to the fake handlers.</summary>
    public ISender Sender()
    {
        Handlers.AddSingleton<IRequestHandler<GrpcEcho, string>>(Echo);
        Handlers.AddSingleton(Logs.Factory);
        Handlers.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        Handlers.AddMediatR(mediatr => mediatr.RegisterServicesFromAssembly(typeof(Unit).Assembly)); // no handler: they are the fakes above
        return Handlers.BuildServiceProvider().GetRequiredService<ISender>();
    }

    /// <summary>The services as the scenario set them, over the fake handlers.</summary>
    public IReadOnlyList<GrpcService> BuildServices() => Services(Sender());

    /// <summary>A channel to the server built as GrpcServer.Build does, on a test server.</summary>
    public async Task<GrpcChannel> ChannelAsync()
    {
        if (_channel is null)
        {
            _app = GrpcServer.Build(Config, BuildServices(), Logs.Factory, host => host.UseTestServer(), Interceptors);
            await _app.StartAsync();
            _channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = _app.GetTestServer().CreateHandler() });
        }
        return _channel;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _app?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        Logs.Factory.Dispose();
    }
}
