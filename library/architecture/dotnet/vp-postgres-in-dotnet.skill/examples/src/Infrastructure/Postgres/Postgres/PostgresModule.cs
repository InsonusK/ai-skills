// Source: skill vp-postgres-in-dotnet

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Postgres.Abstractions;
using Postgres.Repositories;

namespace Postgres;

/// <summary>The database as the composition root adds it.</summary>
public static class PostgresModule
{
    /// <summary>
    /// Registers <see cref="AppDbContext"/>, one per command scope (BuildingBlocks
    /// <c>AddCommandScope</c>), so the repositories of one command share it. EF Core
    /// reports only what needs a reader: its statements stay out of the log, and
    /// so do the failures a repository turns into its outcomes - a duplicate key is
    /// an answer of the service, not an error record. No retrying execution
    /// strategy: a repeat of a command's statements is its caller's decision.
    /// Its readiness check goes to the framework's registry, which belongs to no
    /// variation point: the health entry point reads it when the application
    /// has one, and nothing runs the check when it has none. The repository is
    /// registered once for every row, so a new table edits no fixed file.
    /// </summary>
    public static IServiceCollection AddPostgres(this IServiceCollection services, PostgresConfig config)
    {
        services.AddLogging(logging => logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning));
        services.AddHealthChecks().AddNpgSql(config.Dsn, name: "database");
        services.AddDbContext<AppDbContext>(options => Configure(options, config));
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());
        return services.AddScoped(typeof(IRepository<>), typeof(PostgresRepository<>));
    }

    /// <summary>The options of every context of the application; <see cref="PostgresMigrations"/> builds its own with them.</summary>
    internal static void Configure(DbContextOptionsBuilder options, PostgresConfig config) =>
        options
            .UseNpgsql(config.Dsn)
            .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.SaveChangesFailed, CoreEventId.QueryIterationFailed, RelationalEventId.CommandError, RelationalEventId.ConnectionError));
}
