using System.Net;
using System.Net.Http.Json;
using Postgres;
using Postgres.Testing;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps;

/// <summary>
/// The whole application: Program.RunAsync on real ports over a real database,
/// so the scenarios go through everything the composition root built.
/// </summary>
[Binding]
public sealed class PersistenceSteps(ITestOutputHelper output)
{
    private static readonly HttpClient Http = new();

    private readonly int _httpPort = FreePort.Next();
    private readonly int _grpcPort = FreePort.Next();
    private readonly StringWriter _stderr = new();
    private CancellationTokenSource? _stop;
    private Task<int>? _running;
    private TestDatabase? _database;
    private HttpResponseMessage? _response;

    private string Items(string listId) => $"http://localhost:{_httpPort}/v1/lists/{listId}/items";

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
        Environment.SetEnvironmentVariable("HTTP_LISTEN_PORT", _httpPort.ToString());
        Environment.SetEnvironmentVariable("GRPC_LISTEN_PORT", _grpcPort.ToString());
        _stop = new CancellationTokenSource();
        _running = Task.Run(() => App.Host.Program.RunAsync(TextWriter.Null, _stderr, _stop.Token));
        var serving = FreePort.WaitServingAsync(_httpPort);
        Assert.Same(serving, await Task.WhenAny(serving, _running));
        await serving;
    }

    [When("the application restarts")]
    public async Task WhenTheApplicationRestarts()
    {
        await StopAsync();
        await WhenTheApplicationStarts();
    }

    [When("the application starts and ends")]
    public async Task WhenTheApplicationStartsAndEnds()
    {
        _running = App.Host.Program.RunAsync(TextWriter.Null, _stderr, CancellationToken.None);
        await _running;
    }

    [When("I add {string} to the list {string} over HTTP")]
    [Given("{string} was added to the list {string} over HTTP")]
    public async Task WhenIAddOverHttp(string title, string listId) =>
        _response = await Http.PostAsJsonAsync(Items(listId), new { title });

    [When("I read the list {string} over HTTP")]
    public async Task WhenIReadOverHttp(string listId) => _response = await Http.GetAsync(Items(listId));

    [Then("the application should answer {int}")]
    public void ThenTheApplicationShouldAnswer(int status) => Assert.Equal((HttpStatusCode)status, _response!.StatusCode);

    [Then("the answer should mention {string}")]
    public async Task ThenTheAnswerShouldMention(string text) => Assert.Contains(text, await _response!.Content.ReadAsStringAsync());

    [Then("the application should have exited with {int}")]
    public async Task ThenTheApplicationShouldHaveExitedWith(int exitCode) => Assert.Equal(exitCode, await _running!);

    [Then("the log should mention {string}")]
    public void ThenTheLogShouldMention(string text) => Assert.Contains(text, _stderr.ToString());

    [Then("the log should have no error record")]
    public void ThenTheLogShouldHaveNoErrorRecord() => Assert.DoesNotContain("\"level\":\"ERROR\"", _stderr.ToString());

    [Then("the log should not mention {string}")]
    public void ThenTheLogShouldNotMention(string text) => Assert.DoesNotContain(text, _stderr.ToString());

    private async Task StopAsync()
    {
        if (_stop is null)
        {
            return;
        }
        await _stop.CancelAsync();
        await _running!;
        _stop.Dispose();
        _stop = null;
    }

    [AfterScenario]
    public async Task StopTheApplication()
    {
        await StopAsync();
        output.WriteLine(_stderr.ToString());
        SetEnvironment(null, null);
        Environment.SetEnvironmentVariable("HTTP_LISTEN_PORT", null);
        Environment.SetEnvironmentVariable("GRPC_LISTEN_PORT", null);
        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }
}
