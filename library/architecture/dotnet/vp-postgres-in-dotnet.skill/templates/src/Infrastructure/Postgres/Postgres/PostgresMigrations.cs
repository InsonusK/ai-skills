// Source: skill vp-postgres-in-dotnet

using Microsoft.EntityFrameworkCore;

namespace Postgres;

/// <summary>Applies the migrations: App.Migrate does it as a job, App.Host at start behind MIGRATE_ON_START.</summary>
public static class PostgresMigrations
{
    /// <summary>
    /// Brings the database up to the latest migration, doing nothing when it is
    /// there already. It throws when the model has changes no migration holds,
    /// so a forgotten make migration stops the deployment instead of the first
    /// query that needs the new column.
    /// </summary>
    public static async Task ApplyAsync(PostgresConfig config, CancellationToken cancellationToken)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>();
        PostgresModule.Configure(options, config);
        await using var context = new AppDbContext(options.Options);
        await context.Database.MigrateAsync(cancellationToken);
    }
}
