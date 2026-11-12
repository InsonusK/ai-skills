using System.Collections.Concurrent;
using App.Host.Api.Core.Entry;
using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Shared.Errors;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps;

[Binding]
public sealed class EntryRunnerSteps(ProcessOutcome outcome, ITestOutputHelper output)
{
    private readonly List<Entry> _entries = [];
    private readonly ConcurrentDictionary<string, bool> _running = new(); // true while an entry's RunAsync has not ended
    private readonly StringWriter _log = new();
    private Exception? _error;

    // Registers an entry that is marked running until its body ends.
    private void Add(string name, Func<CancellationToken, Task> body, bool reportsErrors = false) =>
        _entries.Add(new Entry(name, async stopping =>
        {
            _running[name] = true;
            try
            {
                await body(stopping);
            }
            finally
            {
                _running[name] = false;
            }
        }, reportsErrors));

    // Gives the long-running entries time to start before a one-shot or
    // failing entry ends, so the scenario proves they are stopped, not skipped.
    private static Task WaitOthers() => Task.Delay(20);

    private static ErrorKind KindNamed(string name) => Enum.Parse<ErrorKind>(name.Replace(" ", ""), ignoreCase: true);

    [Given("a long-running entry {string}")]
    public void GivenALongRunningEntry(string name) =>
        Add(name, stopping => Task.Delay(Timeout.Infinite, stopping)); // ends by OperationCanceledException

    [Given("a one-shot entry {string} that succeeds")]
    public void GivenAOneShotEntryThatSucceeds(string name) => Add(name, _ => WaitOthers());

    [Given("an entry {string} that fails with {string}")]
    public void GivenAnEntryThatFails(string name, string message) =>
        Add(name, async _ =>
        {
            await WaitOthers();
            throw new InvalidOperationException(message);
        });

    [Given("a one-shot entry {string} that fails with the kind {string}")]
    public void GivenAOneShotEntryThatFailsWithTheKind(string name, string kind) =>
        Add(name, _ => throw new ErrorKindException(KindNamed(kind), "the reason"));

    [Given("an entry {string} that reports its own error {string} of the kind {string}")]
    public void GivenAnEntryThatReportsItsOwnError(string name, string message, string kind) =>
        Add(name, _ => throw new ErrorKindException(KindNamed(kind), message), reportsErrors: true);

    // Runs the entries and fails the step if the runner does not return in time.
    private async Task Run(CancellationToken stopping)
    {
        using var logging = LoggerFactory.Create(builder => builder.AddProvider(new JsonLogProvider(_log)));
        var running = EntryRunner.RunAllAsync(logging.CreateLogger("test"), stopping, [.. _entries]);
        Assert.Same(running, await Task.WhenAny(running, Task.Delay(TimeSpan.FromSeconds(5))));
        _error = await running;
        outcome.ExitCode = EntryRunner.ExitCode(_error);
        output.WriteLine($"when: RunAllAsync -> {_error?.Message ?? "no error"}, exit {outcome.ExitCode}; log: {_log.ToString().Trim()}");
    }

    [When("the entry points run together")]
    public Task WhenTheEntryPointsRunTogether() => Run(CancellationToken.None);

    [When("the entry points run together and the process is told to terminate")]
    public async Task WhenTheEntryPointsRunAndTerminate()
    {
        using var terminate = new CancellationTokenSource(TimeSpan.FromMilliseconds(50)); // SIGTERM arrives
        await Run(terminate.Token);
    }

    [Then("every entry point should have stopped")]
    public void ThenEveryEntryPointShouldHaveStopped()
    {
        Assert.Equal(_entries.Count, _running.Count); // every entry was started
        Assert.All(_running, entry => Assert.False(entry.Value, $"{entry.Key} still runs"));
    }

    [Then("running should end without an error")]
    public void ThenRunningShouldEndWithoutAnError() => Assert.Null(_error);

    [Then("running should end with the error {string}")]
    public void ThenRunningShouldEndWithTheError(string message) => Assert.Equal(message, _error?.Message);

    [Then("the log should name the entry {string} with its error")]
    public void ThenTheLogShouldNameTheEntry(string name)
    {
        Assert.Contains($"\"entry\":\"{name}\"", _log.ToString());
        Assert.Contains($"\"error\":\"{_error!.Message}\"", _log.ToString());
    }

    [Then("the log should name no entry")]
    public void ThenTheLogShouldNameNoEntry() => Assert.DoesNotContain("\"entry\"", _log.ToString());
}
