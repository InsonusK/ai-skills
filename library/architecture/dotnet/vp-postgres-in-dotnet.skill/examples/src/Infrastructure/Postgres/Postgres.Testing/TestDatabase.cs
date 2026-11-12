// Source: skill vp-postgres-in-dotnet

using Npgsql;

namespace Postgres.Testing;

/// <summary>
/// One database of its own for one scenario, created on the server
/// TEST_DATABASE_DSN points to and dropped after it, so no scenario sees
/// another's rows and scenarios need no order. The project provides that
/// server: a DSN, or a PostgreSQL run inside its dev container.
/// </summary>
public sealed class TestDatabase : IAsyncDisposable
{
    private readonly string _serverDsn;
    private readonly string _name;

    private TestDatabase(string serverDsn, string name, string dsn)
    {
        _serverDsn = serverDsn;
        _name = name;
        Dsn = dsn;
    }

    /// <summary>The DSN of this database.</summary>
    public string Dsn { get; }

    /// <summary>The settings of this database, for a scenario that builds what the composition root builds.</summary>
    public PostgresConfig Config => new() { Dsn = Dsn, MigrateOnStart = false };

    /// <summary>Creates an empty database; a scenario on the stores calls <see cref="PostgresMigrations.ApplyAsync"/> on it, as a deployment would.</summary>
    /// <exception cref="InvalidOperationException">TEST_DATABASE_DSN is not set: a scenario must not pass without its database.</exception>
    public static async Task<TestDatabase> CreateAsync()
    {
        var serverDsn = Environment.GetEnvironmentVariable("TEST_DATABASE_DSN");
        if (string.IsNullOrEmpty(serverDsn))
        {
            throw new InvalidOperationException(
                "TEST_DATABASE_DSN is not set: point it to a PostgreSQL the scenarios may create databases on");
        }
        var name = $"test_{Guid.NewGuid():N}";
        await ExecuteAsync(serverDsn, $"CREATE DATABASE {name}");
        var dsn = new NpgsqlConnectionStringBuilder(serverDsn) { Database = name }.ConnectionString;
        return new TestDatabase(serverDsn, name, dsn);
    }

    /// <summary>Drops the database, whatever connections a scenario left open.</summary>
    public async ValueTask DisposeAsync()
    {
        await using (var connection = new NpgsqlConnection(Dsn))
        {
            NpgsqlConnection.ClearPool(connection);
        }
        await ExecuteAsync(_serverDsn, $"DROP DATABASE {_name} WITH (FORCE)");
    }

    /// <summary>The number of rows of a table, read past every store.</summary>
    public async Task<long> CountAsync(string table)
    {
        await using var connection = new NpgsqlConnection(Dsn);
        await connection.OpenAsync();
        await using var count = new NpgsqlCommand($"SELECT count(*) FROM {table}", connection);
        return (long)(await count.ExecuteScalarAsync())!;
    }

    private static async Task ExecuteAsync(string dsn, string statement)
    {
        await using var connection = new NpgsqlConnection(dsn);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(statement, connection);
        await command.ExecuteNonQueryAsync();
    }
}
