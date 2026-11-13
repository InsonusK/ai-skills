using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Npgsql;
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

    // The item the last add answered: its id and its version, as a caller keeps them.
    private (string Id, string Version)? _added;

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

    [When("I add {string} to the list {string}")]
    [Given("{string} was added to the list {string}")]
    public async Task WhenIAddOverHttp(string title, string listId)
    {
        _response = await Http.PostAsJsonAsync(Items(listId), new { title });
        if (_response.IsSuccessStatusCode)
        {
            var item = await ItemOf(_response);
            _added = (item.GetProperty("id").GetString()!, item.GetProperty("version").GetString()!);
        }
    }

    // The "item" of an answer; proto JSON writes an int64 id as text.
    private static async Task<JsonElement> ItemOf(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("item");

    [When("I complete the added item over HTTP with the version it answered")]
    public async Task WhenICompleteTheAddedItem()
    {
        var (id, version) = _added!.Value;
        _response = await Http.PostAsJsonAsync($"{Items("home")}/{id}/complete", new { version });
    }

    [Given("another connection changes the title of that item")]
    public async Task GivenAnotherConnectionChangesTheTitle()
    {
        await using var connection = new NpgsqlConnection(_database!.Dsn);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("UPDATE list_items SET title = title || ' (renamed)' WHERE id = $1", connection);
        command.Parameters.AddWithValue(long.Parse(_added!.Value.Id));
        Assert.Equal(1, await command.ExecuteNonQueryAsync());
    }

    [Then("the answer should carry another version than the added item's")]
    public async Task ThenTheAnswerShouldCarryAnotherVersion()
    {
        var item = await ItemOf(_response!);
        Assert.True(item.GetProperty("done").GetBoolean());
        Assert.NotEqual(_added!.Value.Version, item.GetProperty("version").GetString());
    }

    [When("I read the list {string}")]
    public async Task WhenIReadOverHttp(string listId) => _response = await Http.GetAsync(Items(listId));

    [Then("the call should succeed")]
    public void ThenTheCallShouldSucceed() => Assert.True((int)_response!.StatusCode is >= 200 and < 300, $"status {_response.StatusCode}: {_response.Content.ReadAsStringAsync().Result}");

    [Then("the call should be rejected as a conflict")]
    public void ThenTheCallShouldBeRejectedAsAConflict() => Assert.Equal(HttpStatusCode.Conflict, _response!.StatusCode);

    [Then("the call should fail as unavailable")]
    public void ThenTheCallShouldFailAsUnavailable() => Assert.Equal(HttpStatusCode.ServiceUnavailable, _response!.StatusCode);

    [Then("the answer should mention {string}")]
    public async Task ThenTheAnswerShouldMention(string text) => Assert.Contains(text, await _response!.Content.ReadAsStringAsync());

    [Then("the application should have exited with {int}")]
    public async Task ThenTheApplicationShouldHaveExitedWith(int exitCode) => Assert.Equal(exitCode, await _running!);

    [Then("the log should mention {string}")]
    public void ThenTheLogShouldMention(string text) => Assert.Contains(text, _stderr.ToString());

    [Then("the log should have no error record")]
    public void ThenTheLogShouldHaveNoErrorRecord() =>
        Assert.False(_stderr.ToString().Contains("\"level\":\"ERROR\""), $"an error record in the log: {_stderr}");

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
