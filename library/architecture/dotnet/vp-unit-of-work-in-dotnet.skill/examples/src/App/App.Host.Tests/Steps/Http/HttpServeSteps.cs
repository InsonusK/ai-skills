using System.Net.Sockets;
using App.Host.Api.Http.Pipeline;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Http;

/// <summary>The HTTP server on a real port, started and stopped as the entry runner does.</summary>
[Binding]
public sealed class HttpServeSteps(HttpWorld world, ITestOutputHelper output) : IDisposable
{
    private readonly CancellationTokenSource _stop = new();
    private Task? _serving;
    private TcpListener? _taken;
    private int _port = FreePort.Next();

    private void Start(Func<Task> serve) => _serving = Task.Run(serve);

    private void StartWithPort() =>
        Start(() => HttpServer.ServeAsync(world.Config with { ListenPort = _port }, _ => { }, world.Logs.Factory, _stop.Token));

    [Given("the HTTP server is serving on a free port")]
    public async Task GivenTheHttpServerIsServing()
    {
        StartWithPort();
        await FreePort.WaitServingAsync(_port);
    }

    [When("the HTTP server is told to stop")]
    public void WhenTheHttpServerIsToldToStop() => _stop.Cancel();

    [Given("the port the HTTP server wants is already taken")]
    public void GivenThePortIsAlreadyTaken() => _taken = FreePort.Take(_port);

    [When("the HTTP server starts")]
    public void WhenTheHttpServerStarts() => StartWithPort();

    [When("the HTTP server starts with the port {string}")]
    public void WhenTheHttpServerStartsWithThePort(string port)
    {
        Environment.SetEnvironmentVariable("HTTP_LISTEN_PORT", port);
        try
        {
            // RunAsync reads the settings before anything listens.
            _serving = HttpServer.RunAsync(_ => { }, world.Logs.Factory, _stop.Token);
        }
        catch (Exception error)
        {
            _serving = Task.FromException(error);
        }
        finally
        {
            Environment.SetEnvironmentVariable("HTTP_LISTEN_PORT", null);
        }
    }

    private async Task<Exception?> EndedWith()
    {
        Assert.Same(_serving, await Task.WhenAny(_serving!, Task.Delay(TimeSpan.FromSeconds(15))));
        try
        {
            await _serving!;
            return null;
        }
        catch (Exception error)
        {
            output.WriteLine($"then: the server stopped with {error.GetType().Name}: {error.Message}");
            return error;
        }
    }

    [Then("the HTTP server should stop without an error")]
    public async Task ThenTheHttpServerShouldStopWithoutAnError() => Assert.Null(await EndedWith());

    [Then("the HTTP server should stop with an error")]
    public async Task ThenTheHttpServerShouldStopWithAnError() => Assert.NotNull(await EndedWith());

    [Then("the HTTP server should stop with an error mentioning {string}")]
    public async Task ThenTheHttpServerShouldStopWithAnErrorMentioning(string text) => Assert.Contains(text, (await EndedWith())?.Message);

    public void Dispose()
    {
        _stop.Cancel();
        _taken?.Stop();
        _stop.Dispose();
    }
}
