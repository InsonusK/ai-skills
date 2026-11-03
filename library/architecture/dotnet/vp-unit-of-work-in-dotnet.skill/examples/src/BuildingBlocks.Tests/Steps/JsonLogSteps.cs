using System.Text.Json;
using System.Text.RegularExpressions;
using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Shared.Errors;
using Xunit;
using Xunit.Abstractions;

namespace BuildingBlocks.Tests.Steps;

[Binding]
public sealed partial class JsonLogSteps(ITestOutputHelper output)
{
    private readonly StringWriter _written = new();
    private LogLevel _level = LogLevel.Information;
    private string? _requestId;

    [Given("the level is {string}")]
    public void GivenTheLevelIs(string level) => _level = Enum.Parse<LogLevel>(level);

    // Logs through the provider as the composition root builds it.
    private void Log(Action<ILogger> write)
    {
        _written.GetStringBuilder().Clear();
        using var factory = LoggerFactory.Create(logging => logging.SetMinimumLevel(_level).AddProvider(new JsonLogProvider(_written)));
        write(factory.CreateLogger("test"));
        output.WriteLine($"when: written {_written.ToString().Trim()}");
    }

    [When("a record {string} is logged with the command {string} and the payload {string}")]
    public void WhenARecordIsLogged(string message, string command, string payload) =>
        Log(logger => logger.Write(LogLevel.Information, message, LogAttr.Of("command", command), LogAttr.Payload(payload)));

    [When("a record {string} is logged with a duration of {int} milliseconds")]
    public void WhenARecordIsLoggedWithADuration(string message, int milliseconds) =>
        Log(logger => logger.Write(LogLevel.Information, message, LogAttr.Of("duration", TimeSpan.FromMilliseconds(milliseconds))));

    [When("a record {string} is logged with an error {string} of no kind")]
    public void WhenARecordIsLoggedWithAnErrorOfNoKind(string message, string error) =>
        Log(logger => logger.Write(LogLevel.Warning, message, Thrown(new InvalidOperationException(error))));

    [When("a record {string} is logged with an error {string} of the kind {string}")]
    public void WhenARecordIsLoggedWithAnErrorOfTheKind(string message, string error, string kind) =>
        Log(logger => logger.Write(LogLevel.Warning, message, Thrown(new ErrorKindException(Enum.Parse<ErrorKind>(kind), error))));

    // An exception as a handler raises it, with a stack.
    private static Exception Thrown(Exception error)
    {
        try
        {
            throw error;
        }
        catch (Exception thrown)
        {
            return thrown;
        }
    }

    private JsonElement Line()
    {
        var lines = _written.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(lines);
        return JsonDocument.Parse(lines[0]).RootElement;
    }

    [Then("the line should have {string} = {string}")]
    public void ThenTheLineShouldHave(string key, string value) =>
        Assert.Equal(value, Line().GetProperty(key) is var property && property.ValueKind == JsonValueKind.String ? property.GetString() : property.GetRawText());

    [Then("the line should have an {string}")]
    public void ThenTheLineShouldHaveAn(string key) => Assert.True(Line().TryGetProperty(key, out _), $"no {key}");

    [Then("the line should have no {string}")]
    public void ThenTheLineShouldHaveNo(string key) => Assert.False(Line().TryGetProperty(key, out _), $"has {key}");

    [Then("the line should not mention {string}")]
    public void ThenTheLineShouldNotMention(string text) => Assert.DoesNotContain(text, _written.ToString());

    [Then("nothing should be written")]
    public void ThenNothingShouldBeWritten() => Assert.Equal("", _written.ToString());

    [When("the request id {string} is begun")]
    public void WhenTheRequestIdIsBegun(string id)
    {
        using (RequestId.Begin(id))
        {
            _requestId = RequestId.Current;
        }
        output.WriteLine($"when: Begin({id}) -> {_requestId}; after: {RequestId.Current ?? "none"}");
        Assert.Null(RequestId.Current); // restored when the call ends
    }

    [Then("the current request id should be {string}")]
    public void ThenTheCurrentRequestIdShouldBe(string id) => Assert.Equal(id, _requestId);

    [Then("the current request id should be 16 hex characters")]
    public void ThenTheCurrentRequestIdShouldBeHex() => Assert.Matches(Hex16(), _requestId);

    [GeneratedRegex("^[0-9a-f]{16}$")]
    private static partial Regex Hex16();
}
