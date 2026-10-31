using Microsoft.Extensions.DependencyInjection;
using Postgres.Abstractions;
using Postgres.Stores;
using Postgres.Testing;
using Reqnroll;

namespace Postgres.Tests.Steps;

/// <summary>
/// The database of one scenario and the stores over it, built as the composition
/// root builds them. Each <see cref="Store"/> call opens a scope of its own, as a
/// command would; <see cref="OneContext"/> keeps one scope for a whole scenario.
/// </summary>
[Binding]
public sealed class PostgresWorld
{
    private readonly List<AsyncServiceScope> _scopes = [];
    private ServiceProvider? _provider;

    public TestDatabase? Database { get; private set; }
    public PostgresConfig? Config { get; private set; }
    public IListStore? OneContext { get; private set; }

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

    public void Use(PostgresConfig config)
    {
        Config = config;
        var services = new ServiceCollection();
        services.AddPostgres(config);
        services.AddScoped<IListStore, PostgresListStore>();
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    public IListStore Store()
    {
        var scope = _provider!.CreateAsyncScope();
        _scopes.Add(scope);
        return scope.ServiceProvider.GetRequiredService<IListStore>();
    }

    public void KeepOneContext() => OneContext = Store();

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
