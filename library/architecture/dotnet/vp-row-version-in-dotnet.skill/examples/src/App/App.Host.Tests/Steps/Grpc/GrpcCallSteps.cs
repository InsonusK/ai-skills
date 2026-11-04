using App.Host.Tests.Steps.Grpc.Contract;
using Grpc.Core;
using Reqnroll;
using Shared.Errors;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Grpc;

/// <summary>Calls of the test method and what the server answered; the echo command's fakes.</summary>
[Binding]
public sealed class GrpcCallSteps(GrpcWorld world, ITestOutputHelper output)
{
    private EchoResponse? _response;
    private RpcException? _error;
    private Metadata? _headers;

    [Given("the gRPC echo command answers {string}")]
    [When("the gRPC echo command answers {string}")]
    public void GivenTheGrpcEchoCommandAnswers(string answer)
    {
        world.Echo.Answer = answer;
        world.Echo.Failure = null;
    }

    [Given("the gRPC echo command is rejected with {string}")]
    public void GivenTheGrpcEchoCommandIsRejected(string message) => world.Echo.Failure = new ErrorKindException(ErrorKind.Invalid, message);

    [Given("the gRPC echo command fails with the kind {string} and {string}")]
    public void GivenTheGrpcEchoCommandFailsWithTheKind(string kind, string message) =>
        world.Echo.Failure = new ErrorKindException(Enum.Parse<ErrorKind>(kind.Replace(" ", ""), ignoreCase: true), message);

    [Given("the gRPC echo command fails with {string}")]
    public void GivenTheGrpcEchoCommandFails(string message) => world.Echo.Failure = new InvalidOperationException(message);

    private async Task Call(string text, string? requestId = null)
    {
        var client = new EchoService.EchoServiceClient(await world.ChannelAsync());
        var headers = requestId is null ? null : new Metadata { { "x-request-id", requestId } };
        _response = null;
        _error = null;
        try
        {
            using var call = client.EchoAsync(new EchoRequest { Text = text }, headers);
            _headers = await call.ResponseHeadersAsync;
            _response = await call.ResponseAsync;
        }
        catch (RpcException error)
        {
            _error = error;
        }
        output.WriteLine($"when: Echo({text}) -> {_response?.Text ?? $"{_error!.StatusCode} {_error.Status.Detail}"}");
    }

    [When("I call the gRPC test method with {string}")]
    public Task WhenICallTheGrpcTestMethod(string text) => Call(text);

    [When("I call the gRPC test method with {string} and the request id {string}")]
    public Task WhenICallTheGrpcTestMethodWithTheRequestId(string text, string requestId) => Call(text, requestId);

    [Then("the gRPC call should succeed with {string}")]
    public void ThenTheGrpcCallShouldSucceedWith(string text)
    {
        Assert.Null(_error);
        Assert.Equal(text, _response!.Text);
    }

    [Then("the gRPC call should fail with {word} and a message containing {string}")]
    public void ThenTheGrpcCallShouldFailWithAMessageContaining(string code, string text)
    {
        Assert.Equal(Enum.Parse<StatusCode>(code), _error?.StatusCode);
        Assert.Contains(text, _error!.Status.Detail);
    }

    [Then("the gRPC call should fail with {word} and the message {string}")]
    public void ThenTheGrpcCallShouldFailWithTheMessage(string code, string message)
    {
        Assert.Equal(Enum.Parse<StatusCode>(code), _error?.StatusCode);
        Assert.Equal(message, _error!.Status.Detail);
    }

    [Then("the gRPC echo command should have received {string}")]
    public void ThenTheGrpcEchoCommandShouldHaveReceived(string text) => Assert.Equal(text, world.Echo.Received);

    [Then("the gRPC echo command should not have been called")]
    public void ThenTheGrpcEchoCommandShouldNotHaveBeenCalled() => Assert.Null(world.Echo.Received);

    [Then("the gRPC server should send back the request id {string}")]
    public void ThenTheGrpcServerShouldSendBackTheRequestId(string id) => Assert.Equal(id, _headers?.GetValue("x-request-id"));

    [Then("the gRPC echo command should have run with the request id {string}")]
    public void ThenTheGrpcEchoCommandShouldHaveRunWithTheRequestId(string id) => Assert.Equal(id, world.Echo.ReceivedRequestId);
}
