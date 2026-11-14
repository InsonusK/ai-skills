using Postgres.Testing;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Migrate.Tests.Steps;

[Binding]
public sealed class MigrateSteps(ITestOutputHelper output)
{
    private readonly StringWriter _stderr = new();
    private TestDatabase? _database;
    private int _exitCode;

    [BeforeScenario]
    public void ClearTheEnvironment() => Environment.SetEnvironmentVariable("DATABASE_DSN", null);

    [Given("an empty database")]
    public async Task GivenAnEmptyDatabase()
    {
        _database = await TestDatabase.CreateAsync();
        Environment.SetEnvironmentVariable("DATABASE_DSN", _database.Dsn);
    }

    [Given("a database that does not answer")]
    public void GivenADatabaseThatDoesNotAnswer() =>
        Environment.SetEnvironmentVariable("DATABASE_DSN", "Host=localhost;Port=1;Database=none;Username=none;Timeout=1");

    [When("the migration job runs")]
    public async Task WhenTheMigrationJobRuns()
    {
        _exitCode = await Program.RunAsync(_stderr, CancellationToken.None);
        output.WriteLine($"when: exit code {_exitCode}: {_stderr}");
    }

    [Then("the job should exit with {int}")]
    public void ThenTheJobShouldExitWith(int exitCode) => Assert.Equal(exitCode, _exitCode);

    [Then("the job should log {string}")]
    public void ThenTheJobShouldLog(string text) => Assert.Contains(text, _stderr.ToString());

    [Then("the table {string} should exist")]
    public async Task ThenTheTableShouldExist(string table) => Assert.Equal(0, await _database!.CountAsync(table));

    [AfterScenario]
    public async Task DropTheDatabase()
    {
        Environment.SetEnvironmentVariable("DATABASE_DSN", null);
        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }
}
