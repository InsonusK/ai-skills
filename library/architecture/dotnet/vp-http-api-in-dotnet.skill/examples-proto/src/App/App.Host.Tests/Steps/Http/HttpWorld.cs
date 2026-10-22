using App.Host.Api.Http.Pipeline;
using BuildingBlocks.Pipeline;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Host.Tests.Steps.Http;

/// <summary>A command of the scenarios, served by the test route POST /v1/echo.</summary>
public sealed record Echo(string Text) : IRequest<string>;

/// <summary>The fake handler of <see cref="Echo"/>: answers what the scenario set, records what it received.</summary>
public sealed class FakeEcho : IRequestHandler<Echo, string>
{
    public string Answer { get; set; } = "";
    public Exception? Failure { get; set; }
    public string? Received { get; private set; }
    public string? ReceivedRequestId { get; private set; }

    public Task<string> Handle(Echo command, CancellationToken cancellationToken)
    {
        Received = command.Text;
        ReceivedRequestId = BuildingBlocks.Logging.RequestId.Current;
        return Failure is null ? Task.FromResult(Answer) : throw Failure;
    }
}

/// <summary>The body and answer of the test route.</summary>
public sealed record EchoBody(string? Text);

/// <summary>
/// The HTTP server of one scenario, built through the fixed pipeline
/// (<see cref="HttpServer.Build"/>) on a test server: its settings, the fake
/// handlers its routes send to, and the records it wrote. The routes are the
/// test route POST /v1/echo (201) unless a scenario sets the application's own.
/// </summary>
public sealed class HttpWorld : IDisposable
{
    private WebApplication? _app;
    private HttpClient? _client;

    public HttpConfig Config { get; set; } = new(8080, "", false);
    public CapturedLogs Logs { get; } = new();
    public FakeEcho Echo { get; } = new();
    /// <summary>The handlers the routes reach, registered before the server is built.</summary>
    public ServiceCollection Handlers { get; } = new();
    /// <summary>Wraps every handler with the base plateau's logging behavior.</summary>
    public bool WithLogging { get; set; }
    /// <summary>The routes under test, given the sender to the fake handlers.</summary>
    public Action<IEndpointRouteBuilder, ISender> Routes { get; set; } = MapEcho;

    public HttpResponseMessage? Response { get; private set; }
    public string ResponseBody { get; private set; } = "";

    // POST /v1/echo: Echo, answered 201.
    private static void MapEcho(IEndpointRouteBuilder routes, ISender sender) =>
        routes.MapPost("/v1/echo", async (EchoBody body, CancellationToken cancellationToken) =>
            TypedResults.Created((string?)null, new EchoBody(await sender.Send(new Echo(body.Text ?? ""), cancellationToken))));

    /// <summary>The sender to the fake handlers, through the logging behavior when asked.</summary>
    public ISender Sender()
    {
        Handlers.AddSingleton<IRequestHandler<Echo, string>>(Echo);
        Handlers.AddSingleton(Logs.Factory);
        Handlers.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        Handlers.AddMediatR(mediatr =>
        {
            mediatr.RegisterServicesFromAssembly(typeof(Unit).Assembly); // no handler: they are the fakes above
            if (WithLogging)
            {
                mediatr.AddOpenBehavior(typeof(LoggingBehavior<,>));
            }
        });
        return Handlers.BuildServiceProvider().GetRequiredService<ISender>();
    }

    /// <summary>Builds the application as HttpServer.Build does, on a test server.</summary>
    public WebApplication BuildApp()
    {
        var sender = Sender();
        return HttpServer.Build(Config, routes => Routes(routes, sender), Logs.Factory, host => host.UseTestServer());
    }

    private async Task<HttpClient> ClientAsync()
    {
        if (_client is null)
        {
            _app = BuildApp();
            await _app.StartAsync();
            _client = _app.GetTestClient();
        }
        return _client;
    }

    /// <summary>Sends a request and keeps the response for the Then steps.</summary>
    public async Task SendAsync(HttpRequestMessage request)
    {
        Response = await (await ClientAsync()).SendAsync(request);
        ResponseBody = await Response.Content.ReadAsStringAsync();
    }

    public void Dispose()
    {
        _client?.Dispose();
        _app?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        Logs.Factory.Dispose();
    }
}
