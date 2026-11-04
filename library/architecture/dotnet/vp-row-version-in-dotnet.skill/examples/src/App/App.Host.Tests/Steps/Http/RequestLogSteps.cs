using System.Text.RegularExpressions;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Http;

/// <summary>The records the HTTP entry point wrote, read with their payload.</summary>
[Binding]
public sealed partial class RequestLogSteps(HttpWorld world, ITestOutputHelper output)
{
    private CapturedRecord Single(string message)
    {
        var records = world.Logs.Records.Where(record => record.Message == message).ToList();
        output.WriteLine($"then: {records.Count} \"{message}\" records");
        return Assert.Single(records);
    }

    private static IDictionary<string, object?> Part(object? value) => Assert.IsAssignableFrom<IDictionary<string, object?>>(value);

    private IDictionary<string, object?> Payload(string message) => Part(Single(message).Attributes["payload"]);

    // Compares only the listed messages, in order, so a record of another part
    // (ASP.NET's own, the server's start) does not break the order.
    [Then("the HTTP records should be, in order, {string}")]
    public void ThenTheHttpRecordsShouldBeInOrder(string messages)
    {
        var listed = messages.Split('|');
        var written = world.Logs.Records.Select(record => record.Message).Where(listed.Contains).ToList();
        Assert.Equal(listed, written);
    }

    [Then("the {string} record should hold the query {string} = {string} and the body {string}")]
    public void ThenTheRecordShouldHoldTheQueryAndTheBody(string message, string name, string value, string body)
    {
        var received = Payload(message);
        var query = Assert.IsAssignableFrom<IDictionary<string, string>>(received["query"]);
        Assert.Equal(value, query.TryGetValue(name, out var got) ? got : "");
        Assert.Equal(body, received["body"]);
    }

    [Then("one {string} record should be logged with:")]
    public void ThenOneRecordShouldBeLoggedWith(string message, DataTable table)
    {
        var record = Single(message);
        foreach (var header in table.Header)
        {
            Assert.Equal(table.Rows[0][header], record.Attributes[header]?.ToString());
        }
    }

    [Then("the {string} record should hold the response body {string}")]
    public void ThenTheRecordShouldHoldTheResponseBody(string message, string body) =>
        Assert.Equal(body, Part(Payload(message)["response"])["body"]);

    [Then("the {string} record should hold no request")]
    public void ThenTheRecordShouldHoldNoRequest(string message) => Assert.False(Payload(message).ContainsKey("request"));

    [Then("the {string} record should hold the request body {string}")]
    public void ThenTheRecordShouldHoldTheRequestBody(string message, string body) =>
        Assert.Equal(body, Part(Payload(message)["request"])["body"]);

    [Then("the HTTP log should hold the error {string} of {string}")]
    public void ThenTheHttpLogShouldHoldTheError(string error, string message) => Assert.Equal(error, Single(message).Exception?.Message);

    private string ResponseRequestId() => Assert.Single(world.Response!.Headers.GetValues("X-Request-Id"));

    [Then("the HTTP response should carry the request id {string}")]
    public void ThenTheHttpResponseShouldCarryTheRequestId(string id) => Assert.Equal(id, ResponseRequestId());

    [Then("the echo command should have run with the request id {string}")]
    public void ThenTheEchoCommandShouldHaveRunWithTheRequestId(string id) => Assert.Equal(id, world.Echo.ReceivedRequestId);

    [Then("the HTTP response should carry a request id of 16 hex characters")]
    public void ThenTheHttpResponseShouldCarryAHexRequestId() => Assert.Matches(Hex16(), ResponseRequestId());

    [GeneratedRegex("^[0-9a-f]{16}$")]
    private static partial Regex Hex16();
}
