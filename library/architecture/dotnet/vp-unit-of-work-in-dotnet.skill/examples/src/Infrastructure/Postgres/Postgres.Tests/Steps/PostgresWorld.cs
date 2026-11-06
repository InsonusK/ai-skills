using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Postgres.Repositories;
using Postgres.Testing;
using Reqnroll;
using Shared.Persistence;

namespace Postgres.Tests.Steps;

/// <summary>
/// The database of one scenario, built as the composition root builds it. Each
/// call of <see cref="Tasks"/> opens a session of its own, as a command would;
/// <see cref="KeepOneSession"/> keeps one for the whole scenario.
/// </summary>
[Binding]
public sealed class PostgresWorld
{
    private readonly List<AsyncServiceScope> _scopes = [];
    private ServiceProvider? _provider;
    private SampleContext? _kept;

    public TestDatabase? Database { get; private set; }
    public PostgresConfig? Config { get; private set; }
    public SampleContext? LastSession { get; private set; }

    public async Task CreateMigratedDatabaseAsync()
    {
        await CreateEmptyDatabaseAsync();
        await PostgresMigrations.ApplyAsync(Config!, CancellationToken.None);
    }

    public async Task CreateEmptyDatabaseAsync()
    {
        Database = await TestDatabase.CreateAsync();
        Use(Database.Config);
    }

    public async Task CreateSampleTablesAsync()
    {
        await CreateEmptyDatabaseAsync();
        await Session().Database.EnsureCreatedAsync();
    }

    public void Use(PostgresConfig config)
    {
        Config = config;
        var services = new ServiceCollection();
        services.AddPostgres(config);
        services.AddDbContext<SampleContext>(options => options.UseNpgsql(config.Dsn));
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    public SampleContext Session()
    {
        if (_kept is not null)
        {
            return _kept;
        }
        var scope = _provider!.CreateAsyncScope();
        _scopes.Add(scope);
        LastSession = scope.ServiceProvider.GetRequiredService<SampleContext>();
        return LastSession;
    }

    public void KeepOneSession() => _kept = Session();

    public TaskRepository Tasks() => new(Session());

    /// <summary>The repository of the application's lists, over a context of its own, as a command gets it.</summary>
    public PostgresRepository<Todo.Domain.Entities.ItemList> Lists()
    {
        var scope = _provider!.CreateAsyncScope();
        _scopes.Add(scope);
        return new(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public PostgresRepository<TRoot> Repository<TRoot>() where TRoot : class, IAggregateRoot => new(Session());

    /// <summary>Runs a statement on a connection of its own, past every repository.</summary>
    public async Task ExecuteAsync(string statement)
    {
        await using var connection = new NpgsqlConnection(Config!.Dsn);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(statement, connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>A number read on a connection of its own, past every repository.</summary>
    public async Task<long> CountAsync(string statement)
    {
        await using var connection = new NpgsqlConnection(Config!.Dsn);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(statement, connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }

    [AfterScenario]
    public async Task DropTheDatabase()
    {
        foreach (var scope in _scopes)
        {
            await scope.DisposeAsync();
        }
        if (_provider is not null)
        {
            await _provider.DisposeAsync();
        }
        if (Database is not null)
        {
            await Database.DisposeAsync();
        }
    }
}
