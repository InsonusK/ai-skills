using System.Diagnostics;
using Postgres;
using Postgres.Testing;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps;

/// <summary>
/// The whole application: a separate CLI process over a real database,
/// so each action goes through everything the composition root builds.
/// </summary>
[Binding]
public sealed class PersistenceSteps(ITestOutputHelper output)
{
    private readonly StringWriter _stderr = new();
    private TestDatabase? _database;
    private int _exitCode;
    private string _answer = "";
    private string _callError = "";

    // A separate OS process for every action proves persistence beyond the
    // lifetime of both the command scope and the application process.
    private async Task RunAsync(params string[] args)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add(typeof(App.Host.Program).Assembly.Location);
        foreach (var arg in args) start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        finally
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
        }
        _exitCode = process.ExitCode;
        _answer = await stdout;
        _callError = await stderr;
        _stderr.Write(_callError);
    }

    private static void SetEnvironment(string? dsn, string? migrate)
    {
        Environment.SetEnvironmentVariable("DATABASE_DSN", dsn);
        Environment.SetEnvironmentVariable("MIGRATE_ON_START", migrate);
    }

    [Given("an empty database for the application")]
    public async Task GivenAnEmptyDatabase()
    {
        _database = await TestDatabase.CreateAsync();
        SetEnvironment(_database.Dsn, null);
    }

    [Given("a migrated database for the application")]
    public async Task GivenAMigratedDatabase()
    {
        await GivenAnEmptyDatabase();
        await PostgresMigrations.ApplyAsync(_database!.Config, CancellationToken.None);
    }

    [Given("a database for the application that does not answer")]
    public void GivenADatabaseThatDoesNotAnswer() =>
        SetEnvironment("Host=localhost;Port=1;Database=none;Username=none;Timeout=1", null);

    [Given("the application migrates on start")]
    public void GivenTheApplicationMigratesOnStart() => Environment.SetEnvironmentVariable("MIGRATE_ON_START", "true");

    [When("the application starts")]
    [Given("the application is running")]
    public async Task WhenTheApplicationStarts()
    {
        await RunAsync("--help");
        Assert.Equal(0, _exitCode);
    }

    [When("the application restarts")]
    public Task WhenTheApplicationRestarts() => WhenTheApplicationStarts();

    [When("the application starts and ends")]
    public Task WhenTheApplicationStartsAndEnds() => RunAsync("--help");

    [When("I add {string} to the list {string}")]
    [Given("{string} was added to the list {string}")]
    public Task WhenIAdd(string title, string listId) =>
        RunAsync("lists", "add", "--list", listId, "--title", title);

    [When("I read the list {string}")]
    public Task WhenIRead(string listId) => RunAsync("lists", "show", "--list", listId);

    [Then("the call should succeed")]
    public void ThenTheCallShouldSucceed() => Assert.True(_exitCode == 0, _callError);

    [Then("the call should be rejected as a conflict")]
    public void ThenTheCallShouldBeRejectedAsAConflict()
    {
        Assert.Equal(2, _exitCode);
        Assert.Empty(_answer);
        Assert.Contains("already", _callError);
    }

    [Then("the call should fail as unavailable")]
    public void ThenTheCallShouldFailAsUnavailable()
    {
        Assert.Equal(1, _exitCode);
        Assert.Empty(_answer);
        Assert.Contains("Error: the database is unavailable", _callError);
    }

    [Then("the answer should mention {string}")]
    public void ThenTheAnswerShouldMention(string text) => Assert.Contains(text, _answer);

    [Then("the application should have exited with {int}")]
    public void ThenTheApplicationShouldHaveExitedWith(int exitCode) => Assert.Equal(exitCode, _exitCode);

    [Then("the log should mention {string}")]
    public void ThenTheLogShouldMention(string text) => Assert.Contains(text, _stderr.ToString());

    [Then("the log should have no error record")]
    public void ThenTheLogShouldHaveNoErrorRecord() =>
        Assert.False(_stderr.ToString().Contains("\"level\":\"ERROR\""), $"an error record in the log: {_stderr}");

    [Then("the log should not mention {string}")]
    public void ThenTheLogShouldNotMention(string text) => Assert.DoesNotContain(text, _stderr.ToString());

    [AfterScenario]
    public async Task StopTheApplication()
    {
        output.WriteLine(_stderr.ToString());
        SetEnvironment(null, null);
        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }
}
