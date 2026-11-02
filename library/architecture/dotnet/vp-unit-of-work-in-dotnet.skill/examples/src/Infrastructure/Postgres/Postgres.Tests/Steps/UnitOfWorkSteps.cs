using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Postgres.Abstractions;
using Postgres.Abstractions.Rows;
using Postgres.Abstractions.Specifications;
using Postgres.Repositories;
using Postgres.Testing;
using Postgres.UnitOfWork;
using Reqnroll;
using Shared.Errors;
using Shared.UnitOfWork;
using Xunit;

namespace Postgres.Tests.Steps;

/// <summary>A table with a statement of its own: it runs past SaveChanges, and the unit of work must still hold it.</summary>
public sealed class ItemRepository(DbContext context) : PostgresRepository<ItemRow>(context)
{
    public Task<int> RenameAsync(string from, string to, CancellationToken cancellationToken) =>
        WriteAsync(
            () => Context.Set<ItemRow>().Where(item => item.Title == from).ExecuteUpdateAsync(item => item.SetProperty(row => row.Title, to), cancellationToken),
            $"rename in {Table}",
            cancellationToken);
}

/// <summary>
/// One command scope over a real database: the repository and the unit of work
/// as a command has them, with the database's table read past both.
/// </summary>
[Binding]
public sealed class UnitOfWorkSteps
{
    private const string ListId = "home";

    private TestDatabase? _database;
    private ServiceProvider? _provider;
    private AsyncServiceScope _scope;
    private ItemRepository _repository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private bool _joined;
    private Exception? _error;
    private IReadOnlyList<ItemRow>? _rows;

    private async Task AttemptAsync(Func<Task> action)
    {
        _error = null;
        try
        {
            await action();
        }
        catch (Exception error)
        {
            _error = error;
        }
    }

    private async Task<long> CountAsync(string statement, params NpgsqlParameter[] parameters)
    {
        await using var connection = new NpgsqlConnection(_database!.Dsn);
        await connection.OpenAsync();
        await using var count = new NpgsqlCommand(statement, connection);
        count.Parameters.AddRange(parameters);
        return (long)(await count.ExecuteScalarAsync())!;
    }

    [Given("a command scope with the unit of work")]
    public async Task GivenACommandScope()
    {
        _database = await TestDatabase.CreateAsync();
        await PostgresMigrations.ApplyAsync(_database.Config, CancellationToken.None);
        var services = new ServiceCollection();
        services.AddPostgres(_database.Config);
        services.AddPostgresUnitOfWork();
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        _scope = _provider.CreateAsyncScope();
        _repository = new ItemRepository(_scope.ServiceProvider.GetRequiredService<AppDbContext>());
        _unitOfWork = _scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    [Given("the list already holds {string}")]
    public async Task GivenTheListAlreadyHolds(string title)
    {
        await using var other = _provider!.CreateAsyncScope();
        await other.ServiceProvider.GetRequiredService<IRepository<ItemRow>>()
            .InsertAsync(new ItemRow { ListId = ListId, Title = title }, CancellationToken.None);
    }

    [Given("the unit of work has begun")]
    public void GivenTheUnitOfWorkHasBegun() => Assert.True(_unitOfWork.TryBegin());

    [When("the command stores {string}")]
    [Given("the command has stored {string}")]
    public Task WhenTheCommandStores(string title) =>
        AttemptAsync(() => _repository.InsertAsync(new ItemRow { ListId = ListId, Title = title }, CancellationToken.None));

    [When("the command reads the list")]
    public async Task WhenTheCommandReadsTheList() =>
        _rows = await _repository.SelectBy(new ItemsOfList(ListId)).OrderBy(row => row.Id).AllAsync(CancellationToken.None);

    [When("the command marks the rows of the list as done")]
    public Task WhenTheCommandMarksTheRows() =>
        AttemptAsync(() => _repository.SelectBy(new ItemsOfList(ListId)).UpdateAsync(row => row.Done = true, CancellationToken.None));

    [When("the command deletes the rows of the list")]
    public Task WhenTheCommandDeletesTheRows() =>
        AttemptAsync(() => _repository.SelectBy(new ItemsOfList(ListId)).DeleteAsync(CancellationToken.None));

    [When("the command renames {string} to {string} in one statement")]
    public Task WhenTheCommandRenames(string from, string to) =>
        AttemptAsync(() => _repository.RenameAsync(from, to, CancellationToken.None));

    [When("a nested command asks to begin")]
    public void WhenANestedCommandAsksToBegin() => _joined = !_unitOfWork.TryBegin();

    [When("the unit of work commits")]
    public Task WhenTheUnitOfWorkCommits() => AttemptAsync(() => _unitOfWork.CommitAsync(CancellationToken.None));

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

    [Then("the table should hold {int} done rows for any other connection")]
    public async Task ThenTheTableShouldHoldDone(int rows) =>
        Assert.Equal(rows, await CountAsync("SELECT count(*) FROM list_items WHERE done"));

    [Then("the table should hold no row titled {string} for any other connection")]
    public async Task ThenTheTableShouldHoldNoRowTitled(string title) =>
        Assert.Equal(0, await CountAsync("SELECT count(*) FROM list_items WHERE title = @title", new NpgsqlParameter("title", title)));

    [Then("the command should see the rows:")]
    public void ThenTheCommandShouldSee(Table expected)
    {
        Assert.Equal(expected.RowCount, _rows!.Count);
        foreach (var (row, index) in expected.Rows.Select((row, index) => (row, index)))
        {
            Assert.Equal(row["title"], _rows[index].Title);
            Assert.Equal(row["done"], _rows[index].Done.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
        }
    }

    [Then("the command should have been told a duplicate key")]
    public void ThenTheCommandShouldHaveBeenToldADuplicateKey() => Assert.IsType<DuplicateKeyException>(_error);

    [Then("the nested command should have joined")]
    public void ThenTheNestedCommandShouldHaveJoined() => Assert.True(_joined);

    [Then("the command should hold no transaction")]
    public void ThenTheCommandShouldHoldNoTransaction() =>
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
