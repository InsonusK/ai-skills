using System.Text.Json.Serialization;
using App.Host.Api.Cli;
using App.Host.Api.Cli.Pipeline;
using Reqnroll;
using Shared.Errors;
using Xunit;

namespace App.Host.Tests.Steps.Cli;

// The test resource of the pipeline scenarios: echo, whose command answers a
// configured text, and plain, the same with no text form of its output. It
// exists only in this project: the application never ships it.

/// <summary>The command of the test resource.</summary>
public sealed record EchoCommand(string Text);

/// <summary>Its result: the configured answer.</summary>
public sealed record EchoResult(string Text);

/// <summary>The JSON line of `test echo` and `test plain`.</summary>
public sealed record EchoOutput([property: JsonPropertyName("text")] string Text);

/// <summary>The echo command faked: it answers the configured text, or fails as configured, and records what it was asked.</summary>
public sealed class EchoFake
{
    public string Answer { get; set; } = "";
    public Exception? Error { get; set; }
    public string? Received { get; private set; }
    public int Calls { get; private set; }

    public Task<EchoResult> Handle(EchoCommand command, CancellationToken stopping)
    {
        Calls++;
        Received = command.Text;
        if (Error is not null)
        {
            throw Error;
        }
        return Task.FromResult(new EchoResult(Answer));
    }
}

/// <summary>
/// The steps of cli.feature (a copy never edited): the fixed pipeline over the
/// test resource. Scoped to the feature, as its "I run" builds the test
/// resource. A crash of the action is a fault of its glue - .NET knows no
/// panic beside exceptions, so a handler's exception is its failure and keeps
/// its message, and the scenario's crash is made in the output mapping.
/// </summary>
[Binding]
[Scope(Feature = "Command line")]
public sealed class CliSteps(CliWorld world)
{
    private readonly EchoFake _echo = new();
    private bool _crashes;

    [When("I run {string}")]
    public Task WhenIRun(string args) => world.RunAsync("todo", "1.2.3", args, [TestResource()]);
    /// <summary>The test resource: `todo test echo` and `todo test plain`.</summary>
    public Resource TestResource() => new("test", "The test resource of the pipeline scenarios",
    [
        CliCommand.Command<EchoCommand, EchoResult>(
            "echo", "Answer the configured text",
            flags: [new CliFlag("text", "the text sent to the command", Required: true)],
            read: flags => new EchoCommand(flags.Value("text")!),
            handle: _echo.Handle,
            encode: result =>
            {
                if (_crashes)
                {
                    throw new InvalidOperationException("echo output broke");
                }
                return new EchoOutput(result.Text);
            },
            text: result => $"echo: {result.Text}"),
        CliCommand.Command<EchoCommand, EchoResult>(
            "plain", "Answer the configured text, with no text form",
            flags: [new CliFlag("text", "the text sent to the command", Required: true)],
            read: flags => new EchoCommand(flags.Value("text")!),
            handle: _echo.Handle,
            encode: result => new EchoOutput(result.Text)),
    ]);

    [Given("the command answers {string}")]
    public void GivenTheCommandAnswers(string answer) => _echo.Answer = answer;

    [Given("the command is rejected with the kind {string} and {string}")]
    public void GivenTheCommandIsRejected(string kind, string message) =>
        _echo.Error = new ErrorKindException(ParseKind(kind), message);

    [Given("the command is rejected with the kind {string} and {string}, caused by {string}, wrapped in {string}")]
    public void GivenTheCommandIsRejectedWrapped(string kind, string message, string cause, string wrap) =>
        _echo.Error = new Exception(wrap, new ErrorKindException(ParseKind(kind), message, new Exception(cause)));

    [Given("the command fails with {string}")]
    public void GivenTheCommandFails(string message) => _echo.Error = new InvalidOperationException(message);

    [Given("the CLI action crashes")]
    public void GivenTheActionCrashes() => _crashes = true;

    [Then("the command should have received {string}")]
    public void ThenTheCommandShouldHaveReceived(string text) => Assert.Equal(text, _echo.Received);

    [Then("the command should not have been called")]
    public void ThenTheCommandShouldNotHaveBeenCalled() => Assert.Equal(0, _echo.Calls);

    [Then("stdout should be indented JSON with {string} = {string}")]
    public void ThenStdoutShouldBeIndentedJson(string name, string value)
    {
        Assert.StartsWith("{" + Environment.NewLine + "  ", world.Stdout);
        Assert.Contains($"\"{name}\": \"{value}\"", world.Stdout);
    }

    [Then("the records should be, in order, {string}")]
    public void ThenTheRecordsShouldBeInOrder(string messages) =>
        Assert.Equal(messages.Split('|'), world.Logs.Records.Select(record => record.Message));

    [Then("one {string} record should be logged for the action {string}, failed {string}")]
    public void ThenOneRecordShouldBeLogged(string message, string action, string failed)
    {
        var record = Assert.Single(world.Logs.Records, record => record.Message == message);
        Assert.Equal(action, record.Attributes["action"]);
        Assert.Equal(bool.Parse(failed), record.Attributes["failed"]);
    }

    [Then("the {string} record should hold the flag {string} = {string}")]
    public void ThenTheRecordShouldHoldTheFlag(string message, string name, string value)
    {
        var record = Assert.Single(world.Logs.Records, record => record.Message == message);
        var flags = Assert.IsType<Dictionary<string, object?>>(record.Attributes["payload"])["flags"];
        Assert.Equal(value, Assert.IsType<Dictionary<string, string>>(flags)[name]);
    }

    [Then("the {string} record should hold the output {string}")]
    public void ThenTheRecordShouldHoldTheOutput(string message, string output)
    {
        var record = Assert.Single(world.Logs.Records, record => record.Message == message);
        Assert.Equal(output, Assert.IsType<Dictionary<string, object?>>(record.Attributes["payload"])["output"]);
    }

    [Then("the {string} record should hold the arguments {string}")]
    public void ThenTheRecordShouldHoldTheArguments(string message, string args)
    {
        var record = Assert.Single(world.Logs.Records, record => record.Message == message);
        Assert.Equal(args, string.Join(" ", Assert.IsAssignableFrom<IEnumerable<string>>(Assert.IsType<Dictionary<string, object?>>(record.Attributes["payload"])["args"])));
    }

    private static ErrorKind ParseKind(string kind) => Enum.Parse<ErrorKind>(kind.Replace(" ", ""), ignoreCase: true);
}
