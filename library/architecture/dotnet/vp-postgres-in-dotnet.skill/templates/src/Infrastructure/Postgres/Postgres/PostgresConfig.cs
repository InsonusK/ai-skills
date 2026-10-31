// Source: skill vp-postgres-in-dotnet

using Npgsql;

namespace Postgres;

/// <summary>
/// The database's settings, read from the environment. A class, not a record: a
/// record would print the DSN, and its password, wherever it is logged.
/// </summary>
public sealed class PostgresConfig
{
    /// <summary>DATABASE_DSN: required, a connection string Npgsql understands.</summary>
    public required string Dsn { get; init; }

    /// <summary>
    /// MIGRATE_ON_START: true applies the migrations before the entry points
    /// start (default false). Only for a deployment pinned to one instance;
    /// every other deployment runs App.Migrate as a job.
    /// </summary>
    public required bool MigrateOnStart { get; init; }

    /// <summary>Reads the settings; throws <see cref="FormatException"/> naming the variable it could not use, never its value.</summary>
    public static PostgresConfig Load()
    {
        var dsn = Environment.GetEnvironmentVariable("DATABASE_DSN");
        if (string.IsNullOrEmpty(dsn))
        {
            throw new FormatException("config: DATABASE_DSN is required");
        }
        string? host;
        try
        {
            host = new NpgsqlConnectionStringBuilder(dsn).Host;
        }
        catch (ArgumentException)
        {
            // Its message quotes the value, which may hold the password.
            throw new FormatException("config: DATABASE_DSN: not a connection string; use Host=...;Port=...;Database=...;Username=...;Password=...");
        }
        if (string.IsNullOrEmpty(host))
        {
            throw new FormatException("config: DATABASE_DSN: names no Host");
        }

        var migrate = Environment.GetEnvironmentVariable("MIGRATE_ON_START");
        return new PostgresConfig
        {
            Dsn = dsn,
            MigrateOnStart = (string.IsNullOrEmpty(migrate) ? "false" : migrate.ToLowerInvariant()) switch
            {
                "true" => true,
                "false" => false,
                _ => throw new FormatException($"config: MIGRATE_ON_START: \"{migrate}\" is neither true nor false"),
            },
        };
    }
}
