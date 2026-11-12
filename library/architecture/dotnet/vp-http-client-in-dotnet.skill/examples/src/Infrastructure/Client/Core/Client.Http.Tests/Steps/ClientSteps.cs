using System.Net;
using System.Text.Json;
using BuildingBlocks.Logging;
using Client.Abstractions;
using Client.Pipeline;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace Client.Http.Tests.Steps;

/// <summary>
/// The request and the response of the test API's one operation, POST
/// /v1/echo - an API of these scenarios only.
/// </summary>
public sealed record Echo(string Text);

[Binding]
public sealed class ClientSteps(ITestOutputHelper output) : IDisposable
{
    private readonly TestApi _api = new();
    private readonly RecordingLogger _logger = new();
    private readonly List<IClientMiddleware> _middlewares = [];
    private readonly List<ClientCall> _seen = [];

    /// <summary>The test API's operations the client may call.</summary>
    private static readonly IReadOnlyList<HttpOperation> Operations = [new(HttpMethod.Post, "/v1/echo", Retryable: true)];

    /// <summary>A client middleware that records the call and sends it twice.</summary>
    private sealed class Twice(List<ClientCall> seen) : IClientMiddleware
    {
        public async Task RunAsync(ClientCall call, ClientAttempt next, CancellationToken cancellationToken)
        {
            seen.Add(call);
            await next(cancellationToken);
            await next(cancellationToken);
        }
    }
    private bool _down;
    private HttpMessageHandler? _emulator;
    private Echo? _answer;
    private Exception? _error;

    // the test API

    [Given("the test API answers {int} with {string}")]
    public void GivenTheTestApiAnswers(int status, string body) => (_api.Status, _api.Body) = (status, body);

    [Given("the test API takes longer than the timeout")]
    public void GivenTheTestApiTakesLonger() => _api.Delay = TimeSpan.FromMilliseconds(300);

    [Given("the test API is not listening")]
    public void GivenTheTestApiIsNotListening() => _down = true;

    [Given("the client of {string} is mocked by an emulator answering {string}")]
    public void GivenTheClientIsMocked(string _, string body) => _emulator = new Emulator(body);

    /// <summary>An emulator that answers every request with one body.</summary>
    private sealed class Emulator(string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) });
    }

    // the call

    /// <summary>The client of the test API: against its server, or, mocked, against the scenario's emulator.</summary>
    private HttpApiClient Client()
    {
        if (_emulator is not null)
        {
            return new HttpApiClient(new HttpApiConfig("test-api", HttpApiConfig.Mock, 80, false, TimeSpan.FromSeconds(1)), _logger, _emulator, Operations, _middlewares);
        }
        if (!_down)
        {
            _api.Start();
        }
        var timeout = _api.Delay > TimeSpan.Zero ? TimeSpan.FromMilliseconds(50) : TimeSpan.FromSeconds(1);
        return new HttpApiClient(new HttpApiConfig("test-api", "127.0.0.1", _api.Port, false, timeout), _logger, new Emulator("unused"), Operations, _middlewares);
    }

    private async Task Call(string method, string path, string text)
    {
        try
        {
            _answer = await Client().CallAsync<Echo>(new HttpMethod(method), path, new Echo(text), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: {method} {path} -> {_answer}, error {_error?.GetType().Name}: {_error?.Message}");
    }

    [When("I call {word} {string} with the text {string}")]
    public Task WhenICall(string method, string path, string text) => Call(method, path, text);

    [When("I call {word} {string} with the text {string} in the request {string}")]
    public async Task WhenICallInTheRequest(string method, string path, string text, string id)
    {
        using (RequestId.Begin(id))
        {
            await Call(method, path, text);
        }
    }

    [Given("a client middleware that sends every call twice")]
    public void GivenAClientMiddlewareThatSendsTwice() => _middlewares.Add(new Twice(_seen));

    [Then("the middleware should have seen the call {string} of {string}, retryable {string}")]
    public void ThenTheMiddlewareShouldHaveSeen(string operation, string api, string retryable) =>
        Assert.Equal(new ClientCall(api, operation, bool.Parse(retryable)), Assert.Single(_seen));

    [Then("the test API should have received {int} requests")]
    public void ThenTheTestApiShouldHaveReceivedRequests(int count) => Assert.Equal(count, _api.Received.Count);

    [Then("the call should succeed with the text {string}")]
    public void ThenTheCallShouldSucceed(string text)
    {
        Assert.Null(_error);
        Assert.Equal(new Echo(text), _answer);
    }

    [Then("the test API should have received {word} {string} with {string}")]
    public void ThenTheTestApiShouldHaveReceived(string method, string path, string body) =>
        Assert.Equal(new Received(method, path, body, null), Assert.Single(_api.Received) with { RequestId = null });

    [Then("the test API should have received the request id {string}")]
    public void ThenTheTestApiShouldHaveReceivedTheRequestId(string id) => Assert.Equal(id, Assert.Single(_api.Received).RequestId);

    [Then("the test API should have received no request id")]
    public void ThenTheTestApiShouldHaveReceivedNoRequestId() => Assert.Null(Assert.Single(_api.Received).RequestId);

    [Then("the call should fail with the status {int}, the code {int}, and the message {string}")]
    public void ThenTheCallShouldFailWithTheStatus(int status, int code, string message)
    {
        var failed = Assert.IsType<ApiStatusException>(_error);
        // A body that is no google.rpc.Status (code 0 in the scenario) is the code Unknown.
        var expected = code == 0 ? ApiStatusCode.Unknown : (ApiStatusCode)code;
        Assert.Equal((status, expected, message), (failed.HttpStatus!.Value, failed.Code, failed.StatusMessage));
    }

    [Then("the call should fail as unavailable")]
    public void ThenTheCallShouldFailAsUnavailable() => Assert.IsType<ApiUnavailableException>(_error);

    [Then("the call should fail mentioning {string}")]
    public void ThenTheCallShouldFailMentioning(string text)
    {
        Assert.IsNotType<ApiUnavailableException>(_error);
        Assert.IsNotType<ApiStatusException>(_error);
        Assert.Contains(text, Assert.IsAssignableFrom<Exception>(_error).Message);
    }

    // CallLog.feature and Mock.feature

    private Record Only(string message) => Assert.Single(_logger.Records, record => record.Message == message);

    private IDictionary<string, object?> Payload(string message) =>
        Assert.IsAssignableFrom<IDictionary<string, object?>>(Only(message).Attributes[LogAttr.PayloadKey]);

    [Then("the records should be, in order, {string}")]
    public void ThenTheRecordsShouldBe(string messages) =>
        Assert.Equal(messages, string.Join("|", _logger.Records.Select(record => record.Message)));

    [Then("the {string} record should hold the request text {string}")]
    public void ThenTheRecordShouldHoldTheRequestText(string message, string text) =>
        Assert.Equal(new Echo(text), Payload(message)["request"]);

    [Then("one \"http client response received\" record should be logged with the API {string}, {word} {string}, the status {int}, and failed {string}")]
    public void ThenOneResponseReceivedRecord(string api, string method, string path, int status, string failed)
    {
        var record = Only("http client response received").Attributes;
        Assert.Equal((api, method, path, status, bool.Parse(failed)),
            ((string)record["api"]!, (string)record["method"]!, (string)record["path"]!, (int)record["status"]!, (bool)record[LogAttr.FailedKey]!));
    }

    [Then("the {string} record should hold the response {string} and no request")]
    public void ThenTheRecordShouldHoldTheResponse(string message, string body)
    {
        var payload = Payload(message);
        Assert.False(payload.ContainsKey("request"));
        Assert.Equal(JsonSerializer.Deserialize<Echo>(body, JsonSerializerOptions.Web), payload["response"]);
    }

    [Then("the \"api client mocked\" record should be a warning naming the API {string}")]
    public void ThenTheMockedRecordShouldBeAWarning(string api)
    {
        var record = Only("api client mocked");
        Assert.Equal((LogLevel.Warning, api), (record.Level, (string)record.Attributes["api"]!));
    }

    public void Dispose() => _api.Dispose();
}
