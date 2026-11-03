using App.Host.Tests.Steps.Grpc.Contract;
using Reqnroll;
using Xunit;

namespace App.Host.Tests.Steps.Grpc;

/// <summary>The records the gRPC entry point wrote, read with their payload.</summary>
[Binding]
public sealed class CallLogSteps(GrpcWorld world)
{
    private CapturedRecord Single(string message) => Assert.Single(world.Logs.Records, record => record.Message == message);

    private IDictionary<string, object?> Payload(string message) =>
        Assert.IsAssignableFrom<IDictionary<string, object?>>(Single(message).Attributes["payload"]);

    // Compares only the listed messages, in order.
    [Then("the gRPC records should be, in order, {string}")]
    public void ThenTheGrpcRecordsShouldBeInOrder(string messages)
    {
        var listed = messages.Split('|');
        Assert.Equal(listed, world.Logs.Records.Select(record => record.Message).Where(listed.Contains));
    }

    [Then("the {string} record should hold the request {string}")]
    public void ThenTheRecordShouldHoldTheRequest(string message, string text) =>
        Assert.Equal(text, Assert.IsType<EchoRequest>(Payload(message)["request"]).Text);

    [Then("one {string} record should be logged with the method {string}, the code {string}, and failed {string}")]
    public void ThenOneRecordShouldBeLoggedWith(string message, string method, string code, string failed)
    {
        var record = Single(message);
        Assert.Equal(method, record.Attributes["method"]);
        Assert.Equal(code, record.Attributes["code"]);
        Assert.Equal(failed, record.Attributes["failed"]?.ToString());
    }

    [Then("the {string} record should hold the response {string} and no request")]
    public void ThenTheRecordShouldHoldTheResponseAndNoRequest(string message, string text)
    {
        var answered = Payload(message);
        Assert.Equal(text, Assert.IsType<EchoResponse>(answered["response"]).Text);
        Assert.False(answered.ContainsKey("request"));
    }

    [Then("the gRPC log should hold the error {string} of {string}")]
    public void ThenTheGrpcLogShouldHoldTheError(string error, string message) => Assert.Equal(error, Single(message).Exception?.Message);
}
