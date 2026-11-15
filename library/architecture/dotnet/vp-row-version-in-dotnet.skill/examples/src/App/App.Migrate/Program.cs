// Source: skill vp-postgres-in-dotnet

using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;
using Postgres;

namespace App.Migrate;

/// <summary>
/// The migration job of a deployment: it reads DATABASE_DSN, brings the database
/// up to the latest migration, and ends, its exit code telling the orchestrator
/// whether the service may start.
/// </summary>
public static class Program
{
    public const int ExitOk = 0;
    public const int ExitFailure = 1;

    public static Task<int> Main() => RunAsync(Console.Error, CancellationToken.None);

    public static async Task<int> RunAsync(TextWriter stderr, CancellationToken cancellationToken)
    {
        using var logging = LoggerFactory.Create(builder => builder.AddProvider(new JsonLogProvider(stderr)));
        var logger = logging.CreateLogger("App.Migrate");
        try
        {
            await PostgresMigrations.ApplyAsync(PostgresConfig.Load(), cancellationToken);
        }
        catch (Exception error)
        {
            logger.Write(LogLevel.Error, "migration failed", error);
            return ExitFailure;
        }
        logger.Write(LogLevel.Information, "migrations applied");
        return ExitOk;
    }
}
