using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Postgres.Abstractions;
using Postgres.Stores;
using Postgres.Testing;
using Postgres.UnitOfWork;
using Reqnroll;
using Shared.Errors;
using Shared.UnitOfWork;
using Xunit;

namespace Postgres.Tests.Steps;

/// <summary>
/// One command scope over a real database: the store and the unit of work as a
/// command has them, with the database's table read past both.
/// </summary>
[Binding]
public sealed class UnitOfWorkSteps
{
    private TestDatabase? _database;
    private ServiceProvider? _provider;
    private AsyncServiceScope _scope;
    private IListStore _store = null!;
    private IUnitOfWork _unitOfWork = null!;
    private bool _joined;
    private Exception? _error;
    private IReadOnlyList<ListRow>? _rows;

    [Given("a command scope with the unit of work")]
    public async Task GivenACommandScope()
    {
        _database = await TestDatabase.CreateAsync();
        await PostgresMigrations.ApplyAsync(_database.Config, CancellationToken.None);
        var services = new ServiceCollection();
        services.AddPostgres(_database.Config);
        services.AddPostgresUnitOfWork();
        services.AddScoped<IListStore, PostgresListStore>();
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        _scope = _provider.CreateAsyncScope();
        _store = _scope.ServiceProvider.GetRequiredService<IListStore>();
        _unitOfWork = _scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    [Given("the unit of work has begun")]
    public void GivenTheUnitOfWorkHasBegun() => Assert.True(_unitOfWork.TryBegin());

    [When("the command stores {string}")]
    [Given("the command has stored {string}")]
    public async Task WhenTheCommandStores(string title)
    {
        _error = null;
        try
        {
            await _store.InsertAsync("home", new ListRow(title, false), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
    }

    [When("the command reads the list")]
    public async Task WhenTheCommandReadsTheList() => _rows = await _store.SelectAsync("home", CancellationToken.None);

    [When("a nested command asks to begin")]
    public void WhenANestedCommandAsksToBegin() => _joined = !_unitOfWork.TryBegin();

    [When("the unit of work commits")]
    public async Task WhenTheUnitOfWorkCommits()
    {
        _error = null;
        try
        {
            await _unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
    }

    [When("the unit of work rolls back")]
    public Task WhenTheUnitOfWorkRollsBack() => _unitOfWork.RollbackAsync(CancellationToken.None);

    [When("the database drops the connection of the command")]
    public async Task WhenTheDatabaseDropsTheConnection()
    {
        var database = new NpgsqlConnectionStringBuilder(_database!.Dsn).Database;
        await using var connection = new NpgsqlConnection(_database.Dsn);
        await connection.OpenAsync();
        await using var terminate = new NpgsqlCommand(
            $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{database}' AND pid <> pg_backend_pid()", connection);
        await terminate.ExecuteNonQueryAsync();
    }

    [Then("the table should hold {int} rows for any other connection")]
    public async Task ThenTheTableShouldHold(int rows) => Assert.Equal(rows, await _database!.CountAsync("list_items"));

    [Then("the command should see {string}")]
    public void ThenTheCommandShouldSee(string titles) =>
        Assert.Equal(titles.Split('|'), _rows!.Select(row => row.Title));

    [Then("the store should have answered a duplicate key")]
    public void ThenTheStoreShouldHaveAnsweredADuplicateKey() => Assert.IsType<DuplicateKeyException>(_error);

    [Then("the nested command should have joined")]
    public void ThenTheNestedCommandShouldHaveJoined() => Assert.True(_joined);

    [Then("no transaction should be open")]
    public void ThenNoTransactionShouldBeOpen() =>
        Assert.Null(_scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.CurrentTransaction);

    [Then("the commit should fail as unavailable")]
    public void ThenTheCommitShouldFailAsUnavailable() => Assert.Equal(ErrorKind.Unavailable, ErrorKindException.KindOf(_error));

    [AfterScenario]
    public async Task EndTheScope()
    {
        if (_provider is null)
        {
            return;
        }
        await _scope.DisposeAsync();
        await _provider.DisposeAsync();
        await _database!.DisposeAsync();
    }
}
