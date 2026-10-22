using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace MaskedLogging.Tests.Steps;

/// <summary>
/// The steps of payload-policy.feature and Payload.feature: records logged
/// through <see cref="MaskedLogProvider"/> as the composition root builds it,
/// and the lines it wrote.
/// </summary>
[Binding]
public sealed class PayloadPolicySteps(ITestOutputHelper output)
{
    private static readonly JsonSerializerOptions Written = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private readonly StringWriter _written = new();
    private MaskedLogConfig _config = MaskedLogConfig.Default;
    private LogLevel _level = LogLevel.Information;
    private string? _requestId;
    private ILoggerFactory? _factory;

    [AfterScenario]
    public void DisposeTheFactory() => _factory?.Dispose();

    // One logger per scenario, built on first use from the settings the
    // Given steps chose, so a warning given once is not given again.
    private ILogger Logger()
    {
        _factory ??= LoggerFactory.Create(logging => logging.SetMinimumLevel(_level)
            .AddProvider(new MaskedLogProvider(_written, _level, _config)));
        return _factory.CreateLogger("test");
    }

    // Logs inside the request id of the scenario, as an entry point would.
    private void Log(Action<ILogger> write)
    {
        using (_requestId is null ? null : RequestId.Begin(_requestId))
        {
            write(Logger());
        }
    }

    private void LogRecord(string message, bool failed, object payload, LogLevel level = LogLevel.Information) =>
        Log(logger => logger.Write(level, message, LogAttr.Of("command", "PlanItems"), LogAttr.Failed(failed), LogAttr.Payload(payload)));

    [Given("the log is written from the level {string}")]
    public void GivenTheLevel(string level) => _level = level switch
    {
        "DEBUG" => LogLevel.Debug,
        "INFO" => LogLevel.Information,
        "WARN" => LogLevel.Warning,
        _ => LogLevel.Error,
    };

    [When("a record {string} is logged at ERROR with the payload body {string}")]
    public void WhenARecordAtErrorWithABody(string message, string body) =>
        LogRecord(message, true, new Dictionary<string, object?> { ["body"] = Encoding.UTF8.GetBytes(body) }, LogLevel.Error);

    [Given("the request id {string} is in the context")]
    public void GivenTheRequestId(string id) => _requestId = id;

    [Given("LOG_PAYLOAD is {string}")]
    public void GivenLogPayload(string mode) => _config = _config with { Payload = mode };

    [Given("LOG_PAYLOAD_MAX_BYTES is {int}")]
    public void GivenLogPayloadMaxBytes(int bytes) => _config = _config with { MaxBytes = bytes };

    [When("a record is logged with the payload password {string} and title {string}")]
    public void WhenARecordWithPasswordAndTitle(string password, string title) =>
        LogRecord("command called", false, new Dictionary<string, object?> { ["password"] = password, ["title"] = title });

    [When("a record with failed {string} is logged with the payload title {string}")]
    public void WhenARecordWithFailed(string failed, string title) =>
        LogRecord("command called", failed == "true", new Dictionary<string, object?> { ["title"] = title });

    [When("a record is logged with the payload password {string} and a title of {int} characters")]
    public void WhenARecordWithPasswordAndALongTitle(string password, int length) =>
        LogRecord("command called", false, new Dictionary<string, object?> { ["password"] = password, ["title"] = new string('a', length) });

    [When("a record is logged with the payload title of {int} characters")]
    public void WhenARecordWithALongTitle(int length) =>
        LogRecord("command called", false, new Dictionary<string, object?> { ["title"] = new string('a', length) });

    [When("a record {string} is logged with the payload body {string}")]
    public void WhenARecordWithABody(string message, string body) =>
        LogRecord(message, false, new Dictionary<string, object?> { ["body"] = Encoding.UTF8.GetBytes(body) });

    // Payload.feature: the ways only Microsoft.Extensions.Logging has

    [When("a record is logged with the template {string} and the payload password {string} and title {string}")]
    public void WhenATemplate(string template, string password, string title) =>
        Log(logger => logger.LogInformation(template, new Dictionary<string, object?> { ["password"] = password, ["title"] = title }));

    [When("a record is logged with the template {string} and the address {string}")]
    public void WhenATemplateWithoutPayload(string template, string address) => Log(logger => logger.LogInformation(template, address));

    [When("a record is logged with the state {string}, which is not a list of attributes")]
    public void WhenAPlainState(string state) => Log(logger => logger.Log(LogLevel.Information, default, state, null, static (text, _) => text));

    [When("a record is logged inside a scope holding the payload password {string}")]
    public void WhenInsideAScope(string password) =>
        Log(logger =>
        {
            using (logger.BeginScope(new[] { LogAttr.Payload(new Dictionary<string, object?> { ["password"] = password }) }))
            {
                logger.Write(LogLevel.Information, "command called", LogAttr.Of("command", "PlanItems"));
            }
        });

    // The lines written, each decoded, and its text written again as the
    // provider writes it, so a step finds "<" as it is.
    private List<(JsonObject Line, string Text)> Lines()
    {
        var lines = _written.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(raw => JsonNode.Parse(raw)!.AsObject())
            .Select(line => (line, line.ToJsonString(Written)))
            .ToList();
        output.WriteLine($"then: log\n{string.Join("\n", lines.Select(l => l.Item2))}");
        return lines;
    }

    private static bool IsWarning(JsonObject line) => line["msg"]?.GetValue<string>() == MaskedLogProvider.NotParsedMessage;

    private (JsonObject Line, string Text) Record() => Lines() is { Count: > 0 } lines ? lines[0] : throw new Xunit.Sdk.XunitException("nothing was logged");

    private static string? Text(JsonNode? value) => value?.GetValueKind() == JsonValueKind.String ? value.GetValue<string>() : value?.ToJsonString();

    [Then("the record should have {string} = {string}")]
    public void ThenTheRecordShouldHave(string key, string value) => Assert.Equal(value, Text(Record().Line[key]));

    [Then("the record should have {string}")]
    public void ThenTheRecordShouldHaveKey(string key) => Assert.True(Record().Line.ContainsKey(key), $"no {key}");

    [Then("the record should not have {string}")]
    public void ThenTheRecordShouldNotHaveKey(string key) => Assert.False(Record().Line.ContainsKey(key), $"has {key}");

    [Then("the record should mention {string}")]
    public void ThenTheRecordShouldMention(string text) => Assert.Contains(text, Record().Text);

    [Then("the record should not mention {string}")]
    public void ThenTheRecordShouldNotMention(string text) => Assert.DoesNotContain(text, Record().Text);

    [Then("the log should not mention {string}")]
    public void ThenTheLogShouldNotMention(string text) => Assert.All(Lines(), line => Assert.DoesNotContain(text, line.Text));

    [Then("every record should have {string} = {string}")]
    public void ThenEveryRecordShouldHave(string key, string value) =>
        Assert.All(Lines().Where(l => !IsWarning(l.Line)), line => Assert.Equal(value, Text(line.Line[key])));

    [Then("every warning should have {string} = {string}")]
    public void ThenEveryWarningShouldHave(string key, string value) =>
        Assert.All(Lines().Where(l => IsWarning(l.Line)), line => Assert.Equal(value, Text(line.Line[key])));

    private List<string> Warnings() => Lines().Where(l => IsWarning(l.Line))
        .Select(l => $"{Text(l.Line["level"])} {Text(l.Line["record"])}|{Text(l.Line["field"])}|{Text(l.Line["bytes"])}")
        .ToList();

    [Then("the log should hold these warnings:")]
    public void ThenTheLogShouldHoldTheseWarnings(DataTable table) =>
        Assert.Equal(table.Rows.Select(row => $"WARN {row["record"]}|{row["field"]}|{int.Parse(row["bytes"])}").ToList(), Warnings());

    [Then("the log should hold no warning")]
    public void ThenTheLogShouldHoldNoWarning() => Assert.Empty(Warnings());
}
