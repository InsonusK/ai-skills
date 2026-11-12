using System.Net.Sockets;
using App.Host.Api.Grpc.Pipeline;
using App.Host.Tests.Steps.Grpc.Contract;
using Grpc.Net.Client;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Grpc;

/// <summary>The gRPC server on a real port, started and stopped as the entry runner does.</summary>
[Binding]
public sealed class GrpcServeSteps(GrpcWorld world, ITestOutputHelper output) : IDisposable
{
    private readonly CancellationTokenSource _stop = new();
    private readonly int _port = FreePort.Next();
    private Task? _serving;
    private Task<EchoResponse>? _inFlight;
    private GrpcChannel? _channel;
    private TcpListener? _taken;

    private void StartWithPort() =>
        _serving = Task.Run(() => GrpcServer.ServeAsync(world.Config with { ListenPort = _port }, world.BuildServices(), world.Logs.Factory, _stop.Token));

    [Given("the gRPC echo command takes {int} milliseconds and answers {string}")]
    public void GivenTheGrpcEchoCommandTakes(int milliseconds, string answer)
    {
        world.Echo.Delay = TimeSpan.FromMilliseconds(milliseconds);
        world.Echo.Answer = answer;
    }

    [Given("the gRPC server is serving on a free port")]
    public async Task GivenTheGrpcServerIsServing()
    {
        StartWithPort();
        await FreePort.WaitServingAsync(_port);
    }

    [When("I start a call of the gRPC test method over TCP")]
    public async Task WhenIStartACallOverTcp()
    {
        _channel = GrpcChannel.ForAddress($"http://127.0.0.1:{_port}");
        _inFlight = new EchoService.EchoServiceClient(_channel).EchoAsync(new EchoRequest { Text = "slow" }).ResponseAsync;
        while (world.Echo.Received is null)
        {
            await Task.Delay(10); // the call has reached the command
        }
    }

    [When("the gRPC server is told to stop")]
    public void WhenTheGrpcServerIsToldToStop() => _stop.Cancel();

    [Then("the gRPC call in flight should succeed with {string}")]
    public async Task ThenTheGrpcCallInFlightShouldSucceedWith(string text) => Assert.Equal(text, (await _inFlight!).Text);

    [Given("the port the gRPC server wants is already taken")]
    public void GivenThePortIsAlreadyTaken() => _taken = FreePort.Take(_port);

    [When("the gRPC server starts")]
    public void WhenTheGrpcServerStarts() => StartWithPort();

    [When("the gRPC server starts with the port {string}")]
    public void WhenTheGrpcServerStartsWithThePort(string port)
    {
        Environment.SetEnvironmentVariable("GRPC_LISTEN_PORT", port);
        try
        {
            // RunAsync reads the settings before anything listens.
            _serving = GrpcServer.RunAsync(world.BuildServices(), world.Logs.Factory, _stop.Token);
        }
        catch (Exception error)
        {
            _serving = Task.FromException(error);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GRPC_LISTEN_PORT", null);
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

    [Then("the gRPC server should stop without an error")]
    public async Task ThenTheGrpcServerShouldStopWithoutAnError() => Assert.Null(await EndedWith());

    [Then("the gRPC server should stop with an error")]
    public async Task ThenTheGrpcServerShouldStopWithAnError() => Assert.NotNull(await EndedWith());

    [Then("the gRPC server should stop with an error mentioning {string}")]
    public async Task ThenTheGrpcServerShouldStopWithAnErrorMentioning(string text) => Assert.Contains(text, (await EndedWith())?.Message);

    public void Dispose()
    {
        _stop.Cancel();
        _channel?.Dispose();
        _taken?.Stop();
        _stop.Dispose();
    }
}
