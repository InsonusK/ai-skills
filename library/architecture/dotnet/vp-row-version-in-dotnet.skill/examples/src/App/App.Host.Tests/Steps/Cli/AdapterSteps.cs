using App.Host.Api.Cli.Pipeline;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Shared.Errors;
using Xunit;

namespace App.Host.Tests.Steps.Cli;

[Binding]
public sealed class AdapterSteps
{
    private readonly CapturedLogs _logs = new();
    private Exception? _error;
    private int _calls;

    [When("a CLI adapter throws a format exception in {string}")]
    public async Task WhenAFormatExceptionIsThrown(string stage)
    {
        var action = CliCommand.Command<string, string>("run", "Test adapter", [],
            read: _ => stage == "read" ? throw new FormatException("adapter broke") : "input",
            handle: (_, _) =>
            {
                _calls++;
                return stage == "handler" ? throw new FormatException("adapter broke") : Task.FromResult("answer");
            },
            encode: result => stage == "encode" ? throw new FormatException("adapter broke") : result,
            text: result => stage == "text" ? throw new FormatException("adapter broke") : result);
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        _error = await App.Host.Api.Cli.Pipeline.Cli.RunAsync(_logs.Factory.CreateLogger("test"), "test", "1",
            ["resource", "run", "--output", "text"], [new Resource("resource", "Test", [action])],
            stdout, stderr, CancellationToken.None);
        Assert.Empty(stdout.ToString());
    }

    [Then("the adapter failure should have kind {string} and message {string}")]
    public void ThenTheFailureHasMeaning(string kind, string message)
    {
        Assert.Equal(kind == "none" ? (ErrorKind?)null : Enum.Parse<ErrorKind>(kind), ErrorKindException.KindOf(_error));
        Assert.Equal(message, _error?.Message);
    }

    [Then("the adapter handler should have been called {int} times")]
    public void ThenTheHandlerWasCalled(int calls) => Assert.Equal(calls, _calls);

    [Then("the adapter should have logged {int} crashes")]
    public void ThenCrashesWereLogged(int crashes) => Assert.Equal(crashes, _logs.Records.Count(record => record.Message == "cli action crashed"));
}
