// Source: skill vp-postgres-in-dotnet

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Postgres;

/// <summary>The database as the composition root adds it.</summary>
public static class PostgresModule
{
    /// <summary>
    /// Registers <see cref="AppDbContext"/>, one per command scope (BuildingBlocks
    /// <c>AddCommandScope</c>), so the stores of one command share it. EF Core
    /// reports only what needs a reader: its statements stay out of the log, and
    /// so do the failures a store turns into its outcomes - a duplicate key is
    /// an answer of the service, not an error record. No retrying execution
    /// strategy: a repeat of a command's statements is its caller's decision.
    /// </summary>
    public static IServiceCollection AddPostgres(this IServiceCollection services, PostgresConfig config)
    {
        services.AddLogging(logging => logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning));
        return services.AddDbContext<AppDbContext>(options => Configure(options, config));
    }

    /// <summary>The options of every context of the application; <see cref="PostgresMigrations"/> builds its own with them.</summary>
    internal static void Configure(DbContextOptionsBuilder options, PostgresConfig config) =>
        options
            .UseNpgsql(config.Dsn)
            .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.SaveChangesFailed, CoreEventId.QueryIterationFailed, RelationalEventId.CommandError));
}
