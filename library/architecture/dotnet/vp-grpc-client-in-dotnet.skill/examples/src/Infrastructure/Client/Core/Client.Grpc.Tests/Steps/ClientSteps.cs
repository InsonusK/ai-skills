using BuildingBlocks.Logging;
using Client.Abstractions;
using Client.Pipeline;
using Google.Protobuf;
using Client.Grpc.Tests.Echo;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace Client.Grpc.Tests.Steps;

[Binding]
public sealed class ClientSteps(ITestOutputHelper output) : IAsyncDisposable
{
    private const string EchoMethod = "/test.v1.EchoService/Echo";

    private readonly TestApi _api = new();
    private readonly RecordingLogger _logger = new();
    private readonly List<IClientMiddleware> _middlewares = [];
    private readonly List<ClientCall> _seen = [];

    /// <summary>The test API's methods the client may call.</summary>
    private static readonly IReadOnlyList<GrpcMethod> Methods = [new(EchoMethod, Retryable: true)];

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
    private bool _slow;
    private Action<ServiceBinderBase>? _emulator;
    private GrpcApiClient? _client;
    private EchoMessage? _answer;
    private Exception? _error;

    /// <summary>The gRPC code of its name in the contract: ALREADY_EXISTS is AlreadyExists.</summary>
    private static StatusCode CodeNamed(string name) =>
        Enum.Parse<StatusCode>(name.Replace("_", ""), ignoreCase: true);

    // the test API

    [Given("the test API answers {string}")]
    public void GivenTheTestApiAnswers(string text) => _api.Echo.Text = text;

    [Given("the test API fails with {word} and the message {string}")]
    public void GivenTheTestApiFails(string code, string message) => _api.Echo.Failure = new RpcException(new Status(CodeNamed(code), message));

    [Given("the test API takes longer than the timeout")]
    public void GivenTheTestApiTakesLonger() => (_slow, _api.Echo.Delay) = (true, TimeSpan.FromMilliseconds(300));

    [Given("the test API is not listening")]
    public void GivenTheTestApiIsNotListening() => _down = true;

    [Given("the client of {string} is mocked by an emulator answering {string}")]
    public void GivenTheClientIsMocked(string _, string text) =>
        _emulator = binder => EchoService.BindService(binder, new EchoBehavior { Text = text });

    // the call

    /// <summary>The client of the test API: against its server, or, mocked, against the scenario's emulator.</summary>
    private async Task<GrpcApiClient> Client()
    {
        if (_emulator is not null)
        {
            return new GrpcApiClient(new GrpcApiConfig("test-api", GrpcApiConfig.Mock, 80, false, TimeSpan.FromSeconds(1)), _logger, _emulator, Methods, _middlewares);
        }
        if (!_down)
        {
            await _api.StartAsync();
        }
        var timeout = _slow ? TimeSpan.FromMilliseconds(50) : TimeSpan.FromSeconds(5);
        return new GrpcApiClient(new GrpcApiConfig("test-api", "127.0.0.1", _api.Port, false, timeout), _logger, _ => { }, Methods, _middlewares);
    }

    private async Task Call(string text)
    {
        _client = await Client();
        try
        {
            _answer = await new EchoService.EchoServiceClient(_client.CallInvoker).EchoAsync(new EchoMessage { Text = text });
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: Echo {text} -> {_answer}, error {_error?.GetType().Name}: {_error?.Message}");
    }

    [When("I call Echo with the text {string}")]
    public Task WhenICallEcho(string text) => Call(text);

    [When("I call Echo with the text {string} in the request {string}")]
    public async Task WhenICallEchoInTheRequest(string text, string id)
    {
        using (RequestId.Begin(id))
        {
            await Call(text);
        }
    }

    [Given("a client middleware that sends every call twice")]
    public void GivenAClientMiddlewareThatSendsTwice() => _middlewares.Add(new Twice(_seen));

    [Then("the middleware should have seen the call {string} of {string}, retryable {string}")]
    public void ThenTheMiddlewareShouldHaveSeen(string operation, string api, string retryable) =>
        Assert.Equal(new ClientCall(api, operation, bool.Parse(retryable)), Assert.Single(_seen));

    [Then("the test API should have received {int} calls")]
    public void ThenTheTestApiShouldHaveReceivedCalls(int count) => Assert.Equal(count, _api.Echo.Received.Count);

    [When("I call the unlisted method {string}")]
    public async Task WhenICallTheUnlistedMethod(string method)
    {
        _client = await Client();
        var unlisted = new Method<EchoMessage, EchoMessage>(MethodType.Unary, "test.v1.EchoService", method.Split('/')[^1],
            Marshallers.Create(message => message.ToByteArray(), EchoMessage.Parser.ParseFrom),
            Marshallers.Create(message => message.ToByteArray(), EchoMessage.Parser.ParseFrom));
        try
        {
            await _client.CallInvoker.AsyncUnaryCall(unlisted, null, new CallOptions(), new EchoMessage { Text = "hi" });
        }
        catch (Exception error)
        {
            _error = error;
        }
    }

    [Then("the call should fail mentioning {string}")]
    public void ThenTheCallShouldFailMentioning(string text) => Assert.Contains(text, Assert.IsAssignableFrom<Exception>(_error).Message);

    [Then("the call should succeed with the text {string}")]
    public void ThenTheCallShouldSucceed(string text)
    {
        Assert.Null(_error);
        Assert.Equal(text, _answer!.Text);
    }

    [Then("the test API should have received Echo with the text {string}")]
    public void ThenTheTestApiShouldHaveReceivedEcho(string text) => Assert.Equal(text, Assert.Single(_api.Echo.Received).Text);

    [Then("the test API should have received the request id {string}")]
    public void ThenTheTestApiShouldHaveReceivedTheRequestId(string id) => Assert.Equal(id, Assert.Single(_api.Echo.Received).RequestId);

    [Then("the test API should have received no request id")]
    public void ThenTheTestApiShouldHaveReceivedNoRequestId() => Assert.Null(Assert.Single(_api.Echo.Received).RequestId);

    [Then("the call should fail with the code {word} and the message {string}")]
    public void ThenTheCallShouldFailWithTheCode(string code, string message)
    {
        var failed = Assert.IsType<ApiStatusException>(_error);
        Assert.Equal(((ApiStatusCode)(int)CodeNamed(code), message, (int?)null), (failed.Code, failed.StatusMessage, failed.HttpStatus));
    }

    [Then("the call should fail as unavailable")]
    public void ThenTheCallShouldFailAsUnavailable() => Assert.IsType<ApiUnavailableException>(_error);

    // CallLog.feature and Mock.feature

    private Record Only(string message) => Assert.Single(_logger.Records, record => record.Message == message);

    private IDictionary<string, object?> Payload(string message) =>
        Assert.IsAssignableFrom<IDictionary<string, object?>>(Only(message).Attributes[LogAttr.PayloadKey]);

    [Then("the records should be, in order, {string}")]
    public void ThenTheRecordsShouldBe(string messages) =>
        Assert.Equal(messages, string.Join("|", _logger.Records.Select(record => record.Message)));

    [Then("the {string} record should hold the request text {string}")]
    public void ThenTheRecordShouldHoldTheRequestText(string message, string text) =>
        Assert.Equal(new EchoMessage { Text = text }, Payload(message)["request"]);

    [Then("one \"grpc client call answered\" record should be logged with the API {string}, the method {string}, the code {string}, and failed {string}")]
    public void ThenOneCallAnsweredRecord(string api, string method, string code, string failed)
    {
        var record = Only("grpc client call answered").Attributes;
        Assert.Equal((api, method, code, bool.Parse(failed)),
            ((string)record["api"]!, (string)record["method"]!, (string)record["code"]!, (bool)record[LogAttr.FailedKey]!));
    }

    [Then("the {string} record should hold the response text {string} and no request")]
    public void ThenTheRecordShouldHoldTheResponse(string message, string text)
    {
        var payload = Payload(message);
        Assert.False(payload.ContainsKey("request"));
        Assert.Equal(new EchoMessage { Text = text }, payload["response"]);
    }

    [Then("the {string} record should hold the status message {string}")]
    public void ThenTheRecordShouldHoldTheStatusMessage(string message, string text) =>
        Assert.Equal(text, Assert.IsType<Status>(Payload(message)["status"]).Detail);

    [Then("the \"api client mocked\" record should be a warning naming the API {string}")]
    public void ThenTheMockedRecordShouldBeAWarning(string api)
    {
        var record = Only("api client mocked");
        Assert.Equal((LogLevel.Warning, api), (record.Level, (string)record.Attributes["api"]!));
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        await _api.DisposeAsync();
    }
}
