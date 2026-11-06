using System.Globalization;
using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Postgres.Testing;
using Postgres.UnitOfWork;
using Reqnroll;
using Shared.Errors;
using Shared.UnitOfWork;
using Xunit;

namespace Postgres.Tests.Steps;

/// <summary>Every task of the one owner of these scenarios, in the order they were stored.</summary>
public sealed class TasksOfOwner : Specification<OwnerTask>
{
    public TasksOfOwner(long ownerId) => Query.Where(task => task.OwnerId == ownerId).OrderBy(task => task.Id);
}

/// <summary>
/// One command scope over a real database: the repository and the unit of work
/// as a command has them, with the table read past both. The scenarios run on
/// the sample table of tasks, the "list" of their text: it has no rule of an
/// aggregate in front of its unique index, so a duplicate key reaches the
/// database and the savepoint of the save is proved. The unit of work watches
/// the saves of that context as AddPostgresUnitOfWork has it watch AppDbContext.
/// </summary>
[Binding]
public sealed class UnitOfWorkSteps
{
    private TestDatabase? _database;
    private ServiceProvider? _provider;
    private AsyncServiceScope _scope;
    private SampleContext _context = null!;
    private TaskRepository _repository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private long _ownerId;
    private bool _joined;
    private Exception? _error;
    private IReadOnlyList<OwnerTask>? _tasks;

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

    private OwnerTask Task(string title) => new() { OwnerId = _ownerId, Title = title };

    [Given("a command scope with the unit of work")]
    public async Task GivenACommandScope()
    {
        _database = await TestDatabase.CreateAsync();
        var services = new ServiceCollection();
        services.AddPostgres(_database.Config);
        services.AddPostgresUnitOfWork();
        services.AddDbContext<SampleContext>((provider, options) =>
            options.UseNpgsql(_database.Config.Dsn).AddInterceptors(provider.GetRequiredService<PostgresUnitOfWork>()));
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        await using (var setup = _provider.CreateAsyncScope())
        {
            var context = setup.ServiceProvider.GetRequiredService<SampleContext>();
            await context.Database.EnsureCreatedAsync();
            var owner = new Owner { Name = "ann" };
            await new Repositories.PostgresRepository<Owner>(context).AddAsync(owner);
            _ownerId = owner.Id;
        }
        _scope = _provider.CreateAsyncScope();
        _context = _scope.ServiceProvider.GetRequiredService<SampleContext>();
        _repository = new TaskRepository(_context);
        _unitOfWork = _scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    [Given("the list already holds {string}")]
    public async Task GivenTheListAlreadyHolds(string title)
    {
        await using var other = _provider!.CreateAsyncScope();
        await new TaskRepository(other.ServiceProvider.GetRequiredService<SampleContext>()).AddAsync(Task(title));
    }

    [Given("the unit of work has begun")]
    public void GivenTheUnitOfWorkHasBegun() => Assert.True(_unitOfWork.TryBegin());

    [When("the command stores {string}")]
    [Given("the command has stored {string}")]
    public Task WhenTheCommandStores(string title) => AttemptAsync(() => _repository.AddAsync(Task(title)));

    [When("the command reads the list")]
    public async Task WhenTheCommandReadsTheList() => _tasks = await _repository.ListAsync(new TasksOfOwner(_ownerId));

    [When("the command marks the rows of the list as done")]
    public Task WhenTheCommandMarksTheRows() =>
        AttemptAsync(async () =>
        {
            var tasks = await _repository.ListAsync(new TasksOfOwner(_ownerId));
            tasks.ForEach(task => task.Done = true);
            await _repository.UpdateRangeAsync(tasks);
        });

    [When("the command deletes the rows of the list")]
    public Task WhenTheCommandDeletesTheRows() =>
        AttemptAsync(() => _repository.DeleteRangeAsync(new TasksOfOwner(_ownerId)));

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
    public async Task ThenTheTableShouldHold(int rows) => Assert.Equal(rows, await _database!.CountAsync("tasks"));

    [Then("the table should hold {int} done rows for any other connection")]
    public async Task ThenTheTableShouldHoldDone(int rows) =>
        Assert.Equal(rows, await CountAsync("SELECT count(*) FROM tasks WHERE done"));

    [Then("the table should hold no row titled {string} for any other connection")]
    public async Task ThenTheTableShouldHoldNoRowTitled(string title) =>
        Assert.Equal(0, await CountAsync("SELECT count(*) FROM tasks WHERE title = @title", new NpgsqlParameter("title", title)));

    [Then("the command should see the rows:")]
    public void ThenTheCommandShouldSee(Table expected)
    {
        Assert.Equal(expected.RowCount, _tasks!.Count);
        foreach (var (row, index) in expected.Rows.Select((row, index) => (row, index)))
        {
            Assert.Equal(row["title"], _tasks[index].Title);
            Assert.Equal(row["done"], _tasks[index].Done.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
        }
    }

    [Then("the command should have been told a duplicate key")]
    public void ThenTheCommandShouldHaveBeenToldADuplicateKey() => Assert.Equal(ErrorKind.Conflict, new PostgresFailures().KindOf(_error!)?.Kind);

    [Then("the nested command should have joined")]
    public void ThenTheNestedCommandShouldHaveJoined() => Assert.True(_joined);

    [Then("the command should hold no transaction")]
    public void ThenTheCommandShouldHoldNoTransaction() => Assert.Null(_context.Database.CurrentTransaction);

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
