using System.Net.Sockets;
using App.Host.Api.Management;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Management;

/// <summary>The management server on a real port, started and stopped as the entry runner does.</summary>
[Binding]
public sealed class ManagementServeSteps(ITestOutputHelper output) : IDisposable
{
    private readonly CapturedLogs _logs = new();
    private readonly CancellationTokenSource _stop = new();
    private readonly int _port = FreePort.Next();
    private Task? _serving;
    private TcpListener? _taken;

    private void StartWithPort() =>
        _serving = Task.Run(() => ManagementServer.ServeAsync(new ManagementConfig(_port), _logs.Factory, _stop.Token));

    [Given("the management server is serving on a free port")]
    public async Task GivenTheManagementServerIsServing()
    {
        StartWithPort();
        await FreePort.WaitServingAsync(_port);
    }

    [When("the management server is told to stop")]
    public void WhenTheManagementServerIsToldToStop() => _stop.Cancel();

    [Given("the port the management server wants is already taken")]
    public void GivenThePortIsAlreadyTaken() => _taken = FreePort.Take(_port);

    [When("the management server starts")]
    public void WhenTheManagementServerStarts() => StartWithPort();

    [When("the management server starts with the port {string}")]
    public void WhenTheManagementServerStartsWithThePort(string port)
    {
        Environment.SetEnvironmentVariable("MANAGEMENT_LISTEN_PORT", port);
        try
        {
            // RunAsync reads the settings before anything listens.
            _serving = ManagementServer.RunAsync(_logs.Factory, _stop.Token);
        }
        catch (Exception error)
        {
            _serving = Task.FromException(error);
        }
        finally
        {
            Environment.SetEnvironmentVariable("MANAGEMENT_LISTEN_PORT", null);
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

    [Then("the management server should stop without an error")]
    public async Task ThenTheManagementServerShouldStopWithoutAnError() => Assert.Null(await EndedWith());

    [Then("the management server should stop with an error")]
    public async Task ThenTheManagementServerShouldStopWithAnError() => Assert.NotNull(await EndedWith());

    [Then("the management server should stop with an error mentioning {string}")]
    public async Task ThenTheManagementServerShouldStopWithAnErrorMentioning(string text) => Assert.Contains(text, (await EndedWith())?.Message);

    public void Dispose()
    {
        _stop.Cancel();
        _taken?.Stop();
        _stop.Dispose();
        _logs.Factory.Dispose();
    }
}
